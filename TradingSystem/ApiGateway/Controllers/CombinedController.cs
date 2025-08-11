using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CombinedController : ControllerBase {
    private readonly IHttpClientFactory _httpClientFactory;

    public CombinedController(IHttpClientFactory httpClientFactory) {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 🚀 Выполнить расчет И автоматически сохранить в базу
    /// Это главный метод для Telegram бота!
    /// </summary>
    [HttpPost("calculate-and-save")]
    public async Task<IActionResult> CalculateAndSave([FromBody] CalculateAndSaveRequest request) {
        try {
            var calculationClient = _httpClientFactory.CreateClient("CalculationService");
            var tradeClient = _httpClientFactory.CreateClient("TradeService");

            // 🧮 Шаг 1: Выполняем расчет
            object calculationRequest = request.CalculationType.ToLower() switch {
                "fixedrate" => new {
                    loanAmount = request.LoanAmount,
                    yearlyRates = request.YearlyRates,
                    calculationType = request.InterestType == "Simple" ? 0 : 1,
                    companyName = request.CompanyName
                },
                "floatingrate" => new {
                    loanAmount = request.LoanAmount,
                    years = request.Years,
                    rates = request.FloatingRates,
                    resetPeriod = request.ResetPeriod,
                    companyName = request.CompanyName
                },
                "ois" => new {
                    notionalAmount = request.LoanAmount,
                    overnightRate = request.OvernightRate,
                    days = request.Days,
                    dayCountConvention = request.DayCountConvention,
                    companyName = request.CompanyName
                },
                _ => throw new ArgumentException("Invalid calculation type")
            };

            var calculationEndpoint = request.CalculationType.ToLower() switch {
                "fixedrate" => "api/FixedRate/calculate",
                "floatingrate" => "api/FloatingRate/calculate",
                "ois" => "api/OIS/calculate",
                _ => throw new ArgumentException("Invalid calculation type")
            };

            var calculationJson = JsonSerializer.Serialize(calculationRequest);
            var calculationContent = new StringContent(calculationJson, Encoding.UTF8, "application/json");

            var calculationResponse = await calculationClient.PostAsync(calculationEndpoint, calculationContent);

            if (!calculationResponse.IsSuccessStatusCode) {
                return BadRequest($"Calculation failed: {await calculationResponse.Content.ReadAsStringAsync()}");
            }

            var calculationResult = await calculationResponse.Content.ReadAsStringAsync();
            var calculationData = JsonSerializer.Deserialize<JsonElement>(calculationResult);

            // 💾 Шаг 2: Сохраняем в базу данных
            var saveRequest = new {
                telegramId = request.TelegramId,
                username = request.Username,
                firstName = request.FirstName,
                lastName = request.LastName,
                companyName = request.CompanyName,
                loanAmount = request.LoanAmount,
                years = request.Years,
                calculationType = request.CalculationType,
                interestType = request.InterestType,
                totalInterest = calculationData.GetProperty("data").GetProperty("totalInterest").GetDecimal(),
                totalPayment = calculationData.GetProperty("data").TryGetProperty("totalPayment", out var totalPayment)
                    ? totalPayment.GetDecimal()
                    : request.LoanAmount + calculationData.GetProperty("data").GetProperty("totalInterest").GetDecimal(),
                calculationData = calculationResult
            };

            var saveJson = JsonSerializer.Serialize(saveRequest);
            var saveContent = new StringContent(saveJson, Encoding.UTF8, "application/json");

            var saveResponse = await tradeClient.PostAsync("api/Trades", saveContent);

            if (!saveResponse.IsSuccessStatusCode) {
                return BadRequest($"Save failed: {await saveResponse.Content.ReadAsStringAsync()}");
            }

            var saveResult = await saveResponse.Content.ReadAsStringAsync();

            // 🎉 Возвращаем объединенный результат
            return Ok(new {
                Success = true,
                Calculation = calculationData,
                SaveResult = JsonSerializer.Deserialize<JsonElement>(saveResult),
                Message = "✅ Calculation completed and trade saved successfully!"
            });
        } catch (Exception ex) {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

public class CalculateAndSaveRequest {
    // User info
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Trade info
    public string CompanyName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public int Years { get; set; }
    public string CalculationType { get; set; } = string.Empty; // "FixedRate", "FloatingRate", "OIS"
    public string? InterestType { get; set; } // "Simple", "Compound"

    // FixedRate specific
    public decimal[]? YearlyRates { get; set; }

    // FloatingRate specific  
    public decimal[]? FloatingRates { get; set; }
    public int ResetPeriod { get; set; }

    // OIS specific
    public decimal OvernightRate { get; set; }
    public int Days { get; set; }
    public int DayCountConvention { get; set; }
}
