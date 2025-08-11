using Microsoft.AspNetCore.Mvc;
using DocumentTransformationService.Models.Document;
// using DocumentTransformationService.Services;  // ВРЕМЕННО ЗАКОММЕНТИРОВАНО

namespace DocumentTransformationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase {
    // private readonly IFpMLParserService _fpmlParser;              // ЗАКОММЕНТИРОВАНО
    // private readonly ITradeTransformationService _transformationService; // ЗАКОММЕНТИРОВАНО
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        // IFpMLParserService fpmlParser,                    // ЗАКОММЕНТИРОВАНО
        // ITradeTransformationService transformationService, // ЗАКОММЕНТИРОВАНО
        ILogger<DocumentController> logger) {
        // _fpmlParser = fpmlParser;              // ЗАКОММЕНТИРОВАНО
        // _transformationService = transformationService; // ЗАКОММЕНТИРОВАНО
        _logger = logger;
    }

    /// <summary>
    /// 📁 Загрузить FpML файл и трансформировать его
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] bool autoCalculate = true,
        [FromForm] string? notes = null) {
        try {
            // Валидация файла
            if (file == null || file.Length == 0) {
                return BadRequest(new { Error = "No file uploaded" });
            }

            // Проверяем расширение файла
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsValidFileExtension(fileExtension)) {
                return BadRequest(new { Error = $"Unsupported file type: {fileExtension}. Expected: .xml, .fpml" });
            }

            // Ограничение размера файла (10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize) {
                return BadRequest(new { Error = $"File too large. Max size: {maxFileSize / 1024 / 1024}MB" });
            }

            // Читаем содержимое файла
            string xmlContent;
            using (var reader = new StreamReader(file.OpenReadStream())) {
                xmlContent = await reader.ReadToEndAsync();
            }

            // Генерируем уникальное имя файла
            var generatedFileName = GenerateFileName(file.FileName);

            _logger.LogInformation("Processing uploaded file: {FileName} (generated: {GeneratedFileName})",
                file.FileName, generatedFileName);

            // ВРЕМЕННАЯ ЗАГЛУШКА - возвращаем успешный ответ без парсинга
            var response = new {
                Success = true,
                Message = "File uploaded successfully! (Parser temporarily disabled)",
                FileInfo = new {
                    OriginalFileName = file.FileName,
                    GeneratedFileName = generatedFileName,
                    FileSizeBytes = file.Length,
                    ContentType = file.ContentType,
                    Notes = notes,
                    AutoCalculate = autoCalculate,
                    XmlContentPreview = xmlContent.Length > 200 ? xmlContent.Substring(0, 200) + "..." : xmlContent
                },
                ProcessedAt = DateTime.UtcNow,
                TransformationId = Guid.NewGuid().ToString()
            };

            _logger.LogInformation("Successfully processed file {FileName}", generatedFileName);

            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error processing uploaded file: {FileName}", file?.FileName);

            return BadRequest(new {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 🔍 Анализ файла без расчета (быстрая проверка)
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeDocument([FromForm] IFormFile file) {
        try {
            if (file == null || file.Length == 0) {
                return BadRequest(new { Error = "No file uploaded" });
            }

            string xmlContent;
            using (var reader = new StreamReader(file.OpenReadStream())) {
                xmlContent = await reader.ReadToEndAsync();
            }

            // ВРЕМЕННАЯ ЗАГЛУШКА
            var isXmlFormat = xmlContent.TrimStart().StartsWith("<?xml") || xmlContent.Contains("<dataDocument");

            return Ok(new {
                FileName = file.FileName,
                FileSizeBytes = file.Length,
                IsValid = isXmlFormat,
                InstrumentType = isXmlFormat ? "InterestRateSwap" : "Unknown",
                SupportedForCalculation = isXmlFormat,
                AnalyzedAt = DateTime.UtcNow,
                Message = "Analysis temporarily simplified - parser disabled"
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error analyzing file: {FileName}", file?.FileName);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 🏥 Health check для сервиса
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health() {
        return Ok(new {
            Service = "Document Transformation Service",
            Status = "Healthy",
            SupportedFormats = new[] { "FpML (.xml, .fpml)" },
            MaxFileSizeMB = 10,
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow,
            Note = "Parser temporarily disabled for testing"
        });
    }

    private bool IsValidFileExtension(string extension) {
        var validExtensions = new[] { ".xml", ".fpml" };
        return validExtensions.Contains(extension);
    }

    private string GenerateFileName(string originalFileName) {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var extension = Path.GetExtension(originalFileName);
        var counter = Random.Shared.Next(1000, 9999);

        return $"fpml_{timestamp}_{counter:D4}{extension}";
    }
}