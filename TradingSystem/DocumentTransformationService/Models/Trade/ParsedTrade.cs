namespace DocumentTransformationService.Models.Trade;

/// <summary>
/// 🏦 Распарсенная сделка из FpML документа
/// </summary>
public class ParsedTrade {
    public string TradeId { get; set; } = string.Empty;
    public DateTime TradeDate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime TerminationDate { get; set; }
    public string InstrumentType { get; set; } = string.Empty; // "InterestRateSwap", "Bond", etc.
    public decimal NotionalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public SwapLeg? FixedLeg { get; set; }
    public SwapLeg? FloatingLeg { get; set; }
    public List<Party> Parties { get; set; } = new();

    /// <summary>
    /// Вычисляет срок сделки в годах
    /// </summary>
    public int GetTermInYears() {
        return (TerminationDate - EffectiveDate).Days / 365;
    }

    /// <summary>
    /// Проверяет является ли сделка свопом
    /// </summary>
    public bool IsSwap() {
        return InstrumentType.Contains("Swap", StringComparison.OrdinalIgnoreCase);
    }
}