using System.Xml.Linq;
using DocumentTransformationService.Models.Trade;
using CalculationService.Core.Enums;

namespace DocumentTransformationService.Services;

public class FpMLParserService : IFpMLParserService {
    private readonly ILogger<FpMLParserService> _logger;

    // Dictionary mapping for XML strings to enums - BANKING PRECISION!
    private static readonly Dictionary<string, DayCountConvention> DayCountMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ACT/360", DayCountConvention.Actual360 },
        { "ACTUAL/360", DayCountConvention.Actual360 },
        { "ACT/365", DayCountConvention.Actual365 },
        { "ACTUAL/365", DayCountConvention.Actual365 },
        { "30/360", DayCountConvention.Thirty360 },
        { "30E/360", DayCountConvention.Thirty360 },
        { "ACT/ACT", DayCountConvention.ActualActual },
        { "ACTUAL/ACTUAL", DayCountConvention.ActualActual }
    };

    public FpMLParserService(ILogger<FpMLParserService> logger) {
        _logger = logger;
    }

    /// <summary>
    /// Parse FpML XML document into structured model
    /// </summary>
    public async Task<ParsedTrade> ParseFpMLAsync(string xmlContent) {
        try {
            var doc = XDocument.Parse(xmlContent);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var trade = new ParsedTrade();

            // Parse main trade information
            ParseTradeHeader(doc, ns, trade);

            // Parse swaps (if IRS)
            if (IsInterestRateSwap(doc, ns)) {
                trade.InstrumentType = "InterestRateSwap";
                ParseSwapStreams(doc, ns, trade);
            }

            // Parse parties
            ParseParties(doc, ns, trade);

            _logger.LogInformation("Successfully parsed FpML document for trade {TradeId}", trade.TradeId);

            return trade;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error parsing FpML document");
            throw new InvalidOperationException($"Failed to parse FpML document: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Check if FpML document is valid
    /// </summary>
    public bool IsValidFpML(string xmlContent) {
        try {
            var doc = XDocument.Parse(xmlContent);

            // Check if it's FpML document
            var root = doc.Root;
            if (root == null) return false;

            var ns = root.GetDefaultNamespace();
            return ns.NamespaceName.Contains("fpml.org", StringComparison.OrdinalIgnoreCase);
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Determine instrument type from FpML
    /// </summary>
    public string GetInstrumentType(string xmlContent) {
        try {
            var doc = XDocument.Parse(xmlContent);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            if (IsInterestRateSwap(doc, ns)) return "InterestRateSwap";
            if (doc.Descendants(ns + "bond").Any()) return "Bond";
            if (doc.Descendants(ns + "fra").Any()) return "ForwardRateAgreement";

            return "Unknown";
        } catch {
            return "Unknown";
        }
    }

    // HELPER: Parse DayCount string to enum
    private static DayCountConvention? ParseDayCountConvention(string? xmlValue) {
        if (string.IsNullOrEmpty(xmlValue))
            return null;

        return DayCountMappings.TryGetValue(xmlValue, out var result) ? result : null;
    }

    private void ParseTradeHeader(XDocument doc, XNamespace ns, ParsedTrade trade) {
        var tradeHeader = doc.Descendants(ns + "tradeHeader").FirstOrDefault();
        if (tradeHeader == null) return;

        // Trade ID - PRESERVE ORIGINAL!
        var tradeId = tradeHeader.Descendants(ns + "tradeId").FirstOrDefault()?.Value;
        trade.TradeId = tradeId ?? Guid.NewGuid().ToString();
        _logger.LogInformation("Original TradeId preserved: {TradeId}", trade.TradeId);

        // Trade Date
        var tradeDateStr = tradeHeader.Descendants(ns + "tradeDate").FirstOrDefault()?.Value;
        if (DateTime.TryParse(tradeDateStr, out DateTime tradeDate)) {
            trade.TradeDate = tradeDate;
            _logger.LogInformation("Trade Date: {TradeDate}", tradeDate.ToString("yyyy-MM-dd"));
        }
    }

    private void ParseSwapStreams(XDocument doc, XNamespace ns, ParsedTrade trade) {
        var swapStreams = doc.Descendants(ns + "swapStream").ToList();
        _logger.LogInformation("Found {Count} swap streams", swapStreams.Count);

        foreach (var stream in swapStreams) {
            var leg = new SwapLeg();

            // Parse participants
            leg.PayerParty = stream.Descendants(ns + "payerPartyReference").FirstOrDefault()?.Attribute("href")?.Value ?? "";
            leg.ReceiverParty = stream.Descendants(ns + "receiverPartyReference").FirstOrDefault()?.Attribute("href")?.Value ?? "";

            // Parse dates
            ParseSwapDates(stream, ns, trade);

            // Determine leg type and parse accordingly
            if (stream.Descendants(ns + "fixedRateSchedule").Any()) {
                ParseFixedLeg(stream, ns, leg);
                trade.FixedLeg = leg;
                _logger.LogInformation("Fixed leg: Rate={Rate}%, DayCount={DayCount}, PaymentFreq={Freq}",
                    leg.FixedRate, leg.DayCountFraction, leg.PaymentFrequency);
            } else if (stream.Descendants(ns + "floatingRateCalculation").Any()) {
                ParseFloatingLeg(stream, ns, leg);
                trade.FloatingLeg = leg;
                _logger.LogInformation("Floating leg: Index={Index}, Tenor={Tenor}, DayCount={DayCount}",
                    leg.FloatingRateIndex, leg.IndexTenor, leg.DayCountFraction);
            }

            // Parse common data
            ParseCommonLegData(stream, ns, leg, trade);
        }
    }

    private void ParseSwapDates(XElement stream, XNamespace ns, ParsedTrade trade) {
        var calcPeriodDates = stream.Descendants(ns + "calculationPeriodDates").FirstOrDefault();
        if (calcPeriodDates == null) return;

        // Effective Date
        var effectiveDateStr = calcPeriodDates.Descendants(ns + "effectiveDate")
            .Descendants(ns + "unadjustedDate").FirstOrDefault()?.Value;
        if (DateTime.TryParse(effectiveDateStr, out DateTime effectiveDate)) {
            trade.EffectiveDate = effectiveDate;
        }

        // Termination Date
        var terminationDateStr = calcPeriodDates.Descendants(ns + "terminationDate")
            .Descendants(ns + "unadjustedDate").FirstOrDefault()?.Value;
        if (DateTime.TryParse(terminationDateStr, out DateTime terminationDate)) {
            trade.TerminationDate = terminationDate;
        }
    }

    private void ParseFixedLeg(XElement stream, XNamespace ns, SwapLeg leg) {
        leg.LegType = "Fixed";

        // PRESERVE ORIGINAL RATE VALUE - NO CONVERSION!
        var fixedRateStr = stream.Descendants(ns + "fixedRateSchedule")
            .Descendants(ns + "initialValue").FirstOrDefault()?.Value;

        if (decimal.TryParse(fixedRateStr, out decimal fixedRate)) {
            leg.FixedRate = fixedRate; // Keep as 0.00608, not 0.608%
            _logger.LogInformation("Original fixed rate preserved: {Rate}", fixedRate);
        }

        // BANKING PRECISION: Parse Day Count Convention
        var dayCountStr = stream.Descendants(ns + "dayCountFraction").FirstOrDefault()?.Value;
        leg.DayCountFraction = dayCountStr ?? "";
        leg.DayCountConvention = ParseDayCountConvention(dayCountStr);

        _logger.LogInformation("Fixed leg day count: {DayCountStr} -> {DayCountEnum}",
            dayCountStr, leg.DayCountConvention);
    }

    private void ParseFloatingLeg(XElement stream, XNamespace ns, SwapLeg leg) {
        leg.LegType = "Floating";

        var floatingRateCalc = stream.Descendants(ns + "floatingRateCalculation").FirstOrDefault();
        if (floatingRateCalc == null) return;

        // Floating rate index
        leg.FloatingRateIndex = floatingRateCalc.Descendants(ns + "floatingRateIndex").FirstOrDefault()?.Value;

        // Index tenor
        var indexTenor = floatingRateCalc.Descendants(ns + "indexTenor").FirstOrDefault();
        if (indexTenor != null) {
            var multiplier = indexTenor.Descendants(ns + "periodMultiplier").FirstOrDefault()?.Value;
            var period = indexTenor.Descendants(ns + "period").FirstOrDefault()?.Value;
            leg.IndexTenor = $"{multiplier}{period}";
        }

        // BANKING PRECISION: Parse Day Count Convention for floating leg
        var dayCountStr = stream.Descendants(ns + "dayCountFraction").FirstOrDefault()?.Value;
        leg.DayCountFraction = dayCountStr ?? "";
        leg.DayCountConvention = ParseDayCountConvention(dayCountStr);

        _logger.LogInformation("Floating leg day count: {DayCountStr} -> {DayCountEnum}",
            dayCountStr, leg.DayCountConvention);
    }

    private void ParseCommonLegData(XElement stream, XNamespace ns, SwapLeg leg, ParsedTrade trade) {
        // Notional Amount
        var notionalStr = stream.Descendants(ns + "notionalSchedule")
            .Descendants(ns + "initialValue").FirstOrDefault()?.Value;
        if (decimal.TryParse(notionalStr, out decimal notional)) {
            trade.NotionalAmount = notional;
        }

        // Currency - PRESERVE!
        var currency = stream.Descendants(ns + "currency").FirstOrDefault()?.Value;
        if (!string.IsNullOrEmpty(currency)) {
            trade.Currency = currency;
            _logger.LogInformation("Currency preserved: {Currency}", currency);
        }

        // Payment Frequency
        var paymentFreq = stream.Descendants(ns + "paymentFrequency").FirstOrDefault();
        if (paymentFreq != null) {
            var multiplier = paymentFreq.Descendants(ns + "periodMultiplier").FirstOrDefault()?.Value;
            var period = paymentFreq.Descendants(ns + "period").FirstOrDefault()?.Value;
            leg.PaymentFrequency = $"{multiplier}{period}";
        }

        // Business Day Convention
        leg.BusinessDayConvention = stream.Descendants(ns + "businessDayConvention").FirstOrDefault()?.Value ?? "";

        // Business Centers
        var businessCenters = stream.Descendants(ns + "businessCenter").Select(bc => bc.Value).ToList();
        leg.BusinessCenters = businessCenters;
    }

    private void ParseParties(XDocument doc, XNamespace ns, ParsedTrade trade) {
        var parties = doc.Descendants(ns + "party").ToList();

        foreach (var partyElement in parties) {
            var party = new Party {
                PartyId = partyElement.Attribute("id")?.Value ?? "",
                PartyName = partyElement.Descendants(ns + "partyName").FirstOrDefault()?.Value ?? ""
            };

            trade.Parties.Add(party);
        }
    }

    private bool IsInterestRateSwap(XDocument doc, XNamespace ns) {
        return doc.Descendants(ns + "swap").Any();
    }
}