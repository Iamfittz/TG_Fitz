namespace DocumentTransformationService.Models.Document;

/// <summary>
/// 📁 Модель для загрузки файла через multipart/form-data
/// </summary>
public class FileUploadRequest {
    public bool AutoCalculate { get; set; } = true;
    public string? Notes { get; set; } // Опциональные заметки пользователя
}