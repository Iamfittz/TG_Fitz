using System.Xml.Linq;
using DocumentTransformationService.Models.Trade;

namespace DocumentTransformationService.Services;

public class FpMLParserService : IFpMLParserService {
    private readonly ILogger<FpMLParserService> _logger;

    public FpMLParserService(ILogger<FpMLParserService> logger) {
        _logger = logger;
    }

    /// <summary>
    /// 📄 Парсит FpML XML документ в структурированную модель
    /// </summary>
    public async Task<ParsedTrade> ParseFpMLAsync(string xmlContent) {
        try {
            var doc = XDocument.Parse(xmlContent);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var trade = new ParsedTrade();

            // Парсим основную информацию о сделке
            ParseTradeHeader(doc, ns, trade);

            // Парсим свопы (если это IRS)
            if (IsInterestRateSwap(doc, ns)) {
                trade.InstrumentType = "InterestRateSwap";
                ParseSwapStreams(doc, ns, trade);
            }

            // Парсим участников
            ParseParties(doc, ns, trade);

            _logger.LogInformation("Successfully parsed FpML document for trade {TradeId}", trade.TradeId);

            return trade;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error parsing FpML document");
            throw new InvalidOperationException($"Failed to parse FpML document: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// ✅ Проверяет валидность FpML документа
    /// </summary>
    public bool IsValidFpML(string xmlContent) {
        try {
            var doc = XDocument.Parse(xmlContent);

            // Проверяем что это FpML документ
            var root = doc.Root;
            if (root == null) return false;

            var ns = root.GetDefaultNamespace();
            return ns.NamespaceName.Contains("fpml.org", StringComparison.OrdinalIgnoreCase);
        } catch {
            return false;
        }
    }

    /// <summary>
    /// 🔍 Определяет тип инструмента из FpML
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

    private void ParseTradeHeader(XDocument doc, XNamespace ns, ParsedTrade trade) {
        var tradeHeader = doc.Descendants(ns + "tradeHeader").FirstOrDefault();
        if (tradeHeader == null) return;

        // Trade ID
        var tradeId = tradeHeader.Descendants(ns + "tradeId").FirstOrDefault()?.Value;
        trade.TradeId = tradeId ?? Guid.NewGuid().ToString();

        // Trade Date
        var tradeDateStr = tradeHeader.Descendants(ns + "tradeDate").FirstOrDefault()?.Value;
        if (DateTime.TryParse(tradeDateStr, out DateTime tradeDate)) {
            trade.TradeDate = tradeDate;
        }
    }

    private void ParseSwapStreams(XDocument doc, XNamespace ns, ParsedTrade trade) {
        var swapStreams = doc.Descendants(ns + "swapStream").ToList();

        foreach (var stream in swapStreams) {
            var leg = new SwapLeg();

            // Определяем участников
            leg.PayerParty = stream.Descendants(ns + "payerPartyReference").FirstOrDefault()?.Attribute("href")?.Value ?? "";
            leg.ReceiverParty = stream.Descendants(ns + "receiverPartyReference").FirstOrDefault()?.Attribute("href")?.Value ?? "";

            // Парсим даты
            ParseSwapDates(stream, ns, trade);

            // Определяем тип ноги и парсим соответствующие данные
            if (stream.Descendants(ns + "fixedRateSchedule").Any()) {
                ParseFixedLeg(stream, ns, leg);
                trade.FixedLeg = leg;
            } else if (stream.Descendants(ns + "floatingRateCalculation").Any()) {
                ParseFloatingLeg(stream, ns, leg);
                trade.FloatingLeg = leg;
            }

            // Парсим общие данные
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

        var fixedRateStr = stream.Descendants(ns + "fixedRateSchedule")
            .Descendants(ns + "initialValue").FirstOrDefault()?.Value;

        if (decimal.TryParse(fixedRateStr, out decimal fixedRate)) {
            leg.FixedRate = fixedRate * 100; // Конвертируем в проценты
        }
    }

    private void ParseFloatingLeg(XElement stream, XNamespace ns, SwapLeg leg) {
        leg.LegType = "Floating";

        var floatingRateCalc = stream.Descendants(ns + "floatingRateCalculation").FirstOrDefault();
        if (floatingRateCalc == null) return;

        leg.FloatingRateIndex = floatingRateCalc.Descendants(ns + "floatingRateIndex").FirstOrDefault()?.Value;

        var indexTenor = floatingRateCalc.Descendants(ns + "indexTenor").FirstOrDefault();
        if (indexTenor != null) {
            var multiplier = indexTenor.Descendants(ns + "periodMultiplier").FirstOrDefault()?.Value;
            var period = indexTenor.Descendants(ns + "period").FirstOrDefault()?.Value;
            leg.IndexTenor = $"{multiplier}{period}";
        }
    }

    private void ParseCommonLegData(XElement stream, XNamespace ns, SwapLeg leg, ParsedTrade trade) {
        // Day Count Fraction
        leg.DayCountFraction = stream.Descendants(ns + "dayCountFraction").FirstOrDefault()?.Value ?? "";

        // Notional Amount
        var notionalStr = stream.Descendants(ns + "notionalSchedule")
            .Descendants(ns + "initialValue").FirstOrDefault()?.Value;
        if (decimal.TryParse(notionalStr, out decimal notional)) {
            trade.NotionalAmount = notional;
        }

        // Currency
        var currency = stream.Descendants(ns + "currency").FirstOrDefault()?.Value;
        if (!string.IsNullOrEmpty(currency)) {
            trade.Currency = currency;
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