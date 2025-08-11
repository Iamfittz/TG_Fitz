using DocumentTransformationService.Models.Trade;

namespace DocumentTransformationService.Models.Document;

/// <summary>
/// 🔄 Результат трансформации FpML документа
/// </summary>
public class TransformationResult {
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public ParsedTrade? ParsedTrade { get; set; }
    public object? CalculationResult { get; set; } // Результат расчета если AutoCalculate = true
    public string TransformationId { get; set; } = Guid.NewGuid().ToString();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}