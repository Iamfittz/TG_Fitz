// Добавь в TG_Fitz_Bot/Services/ApiGatewayService.cs

using System.Text;
using System.Text.Json;

namespace TG_Fitz.Services;
public class ApiGatewayService {
    private readonly HttpClient _httpClient;

    public ApiGatewayService(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 🚀 Главный метод для всех типов расчетов через Gateway
    /// </summary>
    public async Task<ApiGatewayResponse?> CalculateAndSaveAsync(ApiGatewayRequest request) {
        try {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Combined/calculate-and-save", content);

            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gateway error: {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiGatewayResponse>(responseJson, new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        } catch (Exception ex) {
            throw new Exception($"Failed to call Gateway: {ex.Message}", ex);
        }
    }
}

// Request model для Gateway
public class ApiGatewayRequest {
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
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

// Response model от Gateway
public class ApiGatewayResponse {
    public bool Success { get; set; }
    public JsonElement Calculation { get; set; }
    public JsonElement SaveResult { get; set; }
    public string Message { get; set; } = string.Empty;
}