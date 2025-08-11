using Microsoft.AspNetCore.Mvc;
using DocumentTransformationService.Models.Document;
using DocumentTransformationService.Services;

namespace DocumentTransformationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase {
    private readonly IFpMLParserService _fpmlParser;
    private readonly ITradeTransformationService _transformationService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IFpMLParserService fpmlParser,
        ITradeTransformationService transformationService,
        ILogger<DocumentController> logger) {
        _fpmlParser = fpmlParser;
        _transformationService = transformationService;
        _logger = logger;
    }

    /// <summary>
    /// 📁 Загрузить FpML файл и трансформировать его
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(IFormFile file, bool autoCalculate = true, string? notes = null) {
        try {
            _logger.LogInformation("=== НАЧАЛО ОБРАБОТКИ ФАЙЛА ===");

            // Валидация файла
            if (file == null || file.Length == 0) {
                _logger.LogWarning("Файл не загружен");
                return BadRequest(new { Error = "No file uploaded" });
            }

            _logger.LogInformation("Файл получен: {FileName}, размер: {Size} байт", file.FileName, file.Length);

            // Проверяем расширение файла
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsValidFileExtension(fileExtension)) {
                _logger.LogWarning("Неподдерживаемый тип файла: {Extension}", fileExtension);
                return BadRequest(new { Error = $"Unsupported file type: {fileExtension}. Expected: .xml, .fpml" });
            }

            // Ограничение размера файла (10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize) {
                _logger.LogWarning("Файл слишком большой: {Size} байт", file.Length);
                return BadRequest(new { Error = $"File too large. Max size: {maxFileSize / 1024 / 1024}MB" });
            }

            // Читаем содержимое файла
            string xmlContent;
            using (var reader = new StreamReader(file.OpenReadStream())) {
                xmlContent = await reader.ReadToEndAsync();
            }

            _logger.LogInformation("XML содержимое загружено, длина: {Length} символов", xmlContent.Length);
            _logger.LogInformation("Первые 200 символов XML: {Preview}",
                xmlContent.Length > 200 ? xmlContent.Substring(0, 200) + "..." : xmlContent);

            // Генерируем уникальное имя файла
            var generatedFileName = GenerateFileName(file.FileName);
            _logger.LogInformation("Сгенерировано имя файла: {GeneratedFileName}", generatedFileName);

            // Проверяем валидность FpML
            var isValidFpML = _fpmlParser.IsValidFpML(xmlContent);
            _logger.LogInformation("Результат проверки FpML: {IsValid}", isValidFpML);

            if (!isValidFpML) {
                _logger.LogWarning("XML не является валидным FpML документом");
                return BadRequest(new {
                    Error = "Invalid FpML document",
                    FileInfo = new {
                        FileName = file.FileName,
                        Size = file.Length,
                        ContentPreview = xmlContent.Length > 500 ? xmlContent.Substring(0, 500) + "..." : xmlContent
                    }
                });
            }

            // Определяем тип инструмента
            var instrumentType = _fpmlParser.GetInstrumentType(xmlContent);
            _logger.LogInformation("Тип инструмента: {InstrumentType}", instrumentType);

            // Парсим документ
            _logger.LogInformation("Начинаем парсинг FpML документа...");
            var parsedTrade = await _fpmlParser.ParseFpMLAsync(xmlContent);
            _logger.LogInformation("Парсинг завершен. Trade ID: {TradeId}, Тип: {InstrumentType}, Сумма: {Amount}",
                parsedTrade.TradeId, parsedTrade.InstrumentType, parsedTrade.NotionalAmount);

            object? calculationResult = null;
            string? calculationStatus = "Not requested";

            if (autoCalculate) {
                try {
                    _logger.LogInformation("Начинаем автоматический расчет...");

                    // Трансформируем в запрос для API Gateway
                    var gatewayRequest = await _transformationService.TransformToApiGatewayRequestAsync(parsedTrade);
                    _logger.LogInformation("Создан запрос для Gateway: {CalculationType}, Сумма: {Amount}",
                        gatewayRequest.CalculationType, gatewayRequest.LoanAmount);

                    // Отправляем на расчет
                    calculationResult = await _transformationService.SendToCalculationAsync(gatewayRequest);
                    calculationStatus = "Completed successfully";
                    _logger.LogInformation("Расчет и сохранение в базу завершены успешно");
                } catch (Exception calcEx) {
                    calculationStatus = $"Failed: {calcEx.Message}";
                    _logger.LogWarning(calcEx, "Не удалось выполнить расчет, но документ распарсен успешно");
                }
            }

            var response = new {
                Success = true,
                Message = "Document processed successfully",
                FileInfo = new {
                    OriginalFileName = file.FileName,
                    GeneratedFileName = generatedFileName,
                    Size = file.Length,
                    ProcessedAt = DateTime.UtcNow
                },
                ParsedData = new {
                    IsValidFpML = isValidFpML,
                    InstrumentType = instrumentType,
                    TradeId = parsedTrade.TradeId,
                    NotionalAmount = parsedTrade.NotionalAmount,
                    Currency = parsedTrade.Currency,
                    EffectiveDate = parsedTrade.EffectiveDate,
                    TerminationDate = parsedTrade.TerminationDate,
                    TermInYears = parsedTrade.GetTermInYears(),
                    Parties = parsedTrade.Parties.Select(p => new { p.PartyId, p.PartyName }).ToList(),
                    FixedLeg = parsedTrade.FixedLeg != null ? new {
                        parsedTrade.FixedLeg.LegType,
                        parsedTrade.FixedLeg.FixedRate,
                        parsedTrade.FixedLeg.PaymentFrequency
                    } : null,
                    FloatingLeg = parsedTrade.FloatingLeg != null ? new {
                        parsedTrade.FloatingLeg.LegType,
                        parsedTrade.FloatingLeg.FloatingRateIndex,
                        parsedTrade.FloatingLeg.IndexTenor
                    } : null
                },
                Calculation = new {
                    Status = calculationStatus,
                    AutoCalculate = autoCalculate,
                    Result = calculationResult
                }
            };

            _logger.LogInformation("=== ОБРАБОТКА ФАЙЛА ЗАВЕРШЕНА УСПЕШНО ===");
            return Ok(response);

        } catch (Exception ex) {
            _logger.LogError(ex, "Ошибка при обработке файла: {FileName}", file?.FileName);

            return BadRequest(new {
                Success = false,
                Error = ex.Message,
                Details = ex.InnerException?.Message,
                FileInfo = file != null ? new {
                    FileName = file.FileName,
                    Size = file.Length
                } : null
            });
        }
    }

    /// <summary>
    /// 🔍 Анализ файла без расчета (быстрая проверка)
    /// </summary>
    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AnalyzeDocument(IFormFile file) {
        try {
            if (file == null || file.Length == 0) {
                return BadRequest(new { Error = "No file uploaded" });
            }

            string xmlContent;
            using (var reader = new StreamReader(file.OpenReadStream())) {
                xmlContent = await reader.ReadToEndAsync();
            }

            var isValid = _fpmlParser.IsValidFpML(xmlContent);
            var instrumentType = isValid ? _fpmlParser.GetInstrumentType(xmlContent) : "Unknown";

            return Ok(new {
                FileName = file.FileName,
                FileSizeBytes = file.Length,
                IsValidFpML = isValid,
                InstrumentType = instrumentType,
                SupportedForCalculation = isValid,
                ContentPreview = xmlContent.Length > 300 ? xmlContent.Substring(0, 300) + "..." : xmlContent,
                AnalyzedAt = DateTime.UtcNow
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
            Timestamp = DateTime.UtcNow
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