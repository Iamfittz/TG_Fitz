using System.Text;
using System.Text.Json;
using DocumentTransformationService.Models.Trade;
using DocumentTransformationService.Models.Gateway;

namespace DocumentTransformationService.Services;

public class TradeTransformationService : ITradeTransformationService {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TradeTransformationService> _logger;

    public TradeTransformationService(IHttpClientFactory httpClientFactory, ILogger<TradeTransformationService> logger) {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// 🔄 Трансформирует ParsedTrade в запрос для ApiGateway
    /// </summary>
    public async Task<ApiGatewayTradeRequest> TransformToApiGatewayRequestAsync(ParsedTrade parsedTrade, long? telegramUserId = null) {
        try {
            var request = new ApiGatewayTradeRequest {
                TelegramId = telegramUserId ?? 999999999, // Default ID для FpML imports
                CompanyName = GetCompanyName(parsedTrade),
                LoanAmount = parsedTrade.NotionalAmount,
                Years = parsedTrade.GetTermInYears(),
                SourceDocument = "FpML",
                TransformationId = Guid.NewGuid().ToString()
            };

            // Определяем тип расчета на основе структуры сделки
            if (parsedTrade.IsSwap() && parsedTrade.FixedLeg != null && parsedTrade.FloatingLeg != null) {
                // Это классический Interest Rate Swap
                request.CalculationType = "FixedRate"; // Упрощаем до Fixed Rate для демо
                request.InterestType = "Simple";

                // Для демо берем фиксированную ставку и создаем массив ставок по годам
                if (parsedTrade.FixedLeg.FixedRate.HasValue) {
                    var fixedRate = parsedTrade.FixedLeg.FixedRate.Value;
                    request.YearlyRates = Enumerable.Repeat(fixedRate, request.Years).ToArray();
                }
            } else if (parsedTrade.FixedLeg != null) {
                // Только фиксированная нога
                request.CalculationType = "FixedRate";
                request.InterestType = "Simple";

                if (parsedTrade.FixedLeg.FixedRate.HasValue) {
                    var fixedRate = parsedTrade.FixedLeg.FixedRate.Value;
                    request.YearlyRates = Enumerable.Repeat(fixedRate, request.Years).ToArray();
                }
            } else if (parsedTrade.FloatingLeg != null) {
                // Плавающая ставка - упрощаем до OIS
                request.CalculationType = "OIS";
                request.OvernightRate = 3.5m; // Default SOFR rate для демо
                request.Days = request.Years * 365;
                request.DayCountConvention = GetDayCountConvention(parsedTrade.FloatingLeg.DayCountFraction);
            }

            _logger.LogInformation("Transformed trade {TradeId} to {CalculationType}",
                parsedTrade.TradeId, request.CalculationType);

            return request;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error transforming trade {TradeId}", parsedTrade.TradeId);
            throw new InvalidOperationException($"Failed to transform trade: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 🧮 Отправляет трансформированную сделку в ApiGateway для расчета
    /// </summary>
    public async Task<object> SendToCalculationAsync(ApiGatewayTradeRequest request) {
        try {
            var httpClient = _httpClientFactory.CreateClient("ApiGateway");

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending calculation request to ApiGateway for transformation {TransformationId}",
                request.TransformationId);

            var response = await httpClient.PostAsync("api/Combined/calculate-and-save", content);

            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"ApiGateway returned {response.StatusCode}: {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<object>(responseJson);

            _logger.LogInformation("Successfully calculated trade via ApiGateway for transformation {TransformationId}",
                request.TransformationId);

            return result ?? new { Error = "Empty response from ApiGateway" };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error sending calculation request to ApiGateway");
            throw new InvalidOperationException($"Failed to send calculation request: {ex.Message}", ex);
        }
    }

    private string GetCompanyName(ParsedTrade parsedTrade) {
        // Пытаемся извлечь название компании из участников
        var firstParty = parsedTrade.Parties.FirstOrDefault();
        if (firstParty != null && !string.IsNullOrEmpty(firstParty.PartyName)) {
            return firstParty.PartyName;
        }

        // Fallback к Trade ID или дефолтному названию
        return !string.IsNullOrEmpty(parsedTrade.TradeId)
            ? $"Trade_{parsedTrade.TradeId}"
            : "FpML_Import";
    }

    private int GetDayCountConvention(string dayCountFraction) {
        return dayCountFraction switch {
            "ACT/360" => 0,
            "ACT/365" => 1,
            "30/360" or "30E/360" => 2,
            "ACT/ACT" => 3,
            _ => 0 // Default to ACT/360
        };
    }
}