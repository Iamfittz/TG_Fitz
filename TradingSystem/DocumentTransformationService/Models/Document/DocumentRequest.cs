namespace DocumentTransformationService.Models.Document;

/// <summary>
/// 📄 Модель для входящего документа
/// </summary>
public class DocumentRequest {
    public string DocumentType { get; set; } = "FpML"; // FpML, SWIFT, FIXML
    public string XmlContent { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public bool AutoCalculate { get; set; } = true; // Автоматически рассчитать после трансформации
    public long? TelegramUserId { get; set; } // Для связи с пользователем бота
}