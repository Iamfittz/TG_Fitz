namespace DocumentTransformationService.Models.Trade;

/// <summary>
/// 🦵 Нога свопа (фиксированная или плавающая)
/// </summary>
public class SwapLeg {
    public string PayerParty { get; set; } = string.Empty;
    public string ReceiverParty { get; set; } = string.Empty;
    public string LegType { get; set; } = string.Empty; // "Fixed", "Floating"

    // Fixed Leg properties
    public decimal? FixedRate { get; set; }

    // Floating Leg properties
    public string? FloatingRateIndex { get; set; } // EUR-LIBOR-BBA, USD-SOFR, etc.
    public string? IndexTenor { get; set; } // 3M, 6M, etc.

    // Common properties
    public string PaymentFrequency { get; set; } = string.Empty; // 6M, 1Y
    public string DayCountFraction { get; set; } = string.Empty; // ACT/360, 30E/360
    public string BusinessDayConvention { get; set; } = string.Empty; // MODFOLLOWING

    public List<string> BusinessCenters { get; set; } = new();
}