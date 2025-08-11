using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase {
    private readonly IHttpClientFactory _httpClientFactory;

    public GatewayController(IHttpClientFactory httpClientFactory) {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 🏥 Health check для Gateway
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health() {
        return Ok(new {
            Service = "API Gateway",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }

    /// <summary>
    /// 🔍 Проверяем доступность всех сервисов
    /// </summary>
    [HttpGet("services-status")]
    public async Task<IActionResult> ServicesStatus() {
        var calculationClient = _httpClientFactory.CreateClient("CalculationService");
        var tradeClient = _httpClientFactory.CreateClient("TradeService");

        var statuses = new {
            Gateway = new { Status = "Online", Port = 7273 },
            CalculationService = await CheckServiceHealth(calculationClient, "api/health"),
            TradeManagementService = await CheckServiceHealth(tradeClient, "api/trades/user/1")
        };

        return Ok(statuses);
    }

    private async Task<object> CheckServiceHealth(HttpClient client, string endpoint) {
        try {
            var response = await client.GetAsync(endpoint);
            return new {
                Status = response.IsSuccessStatusCode ? "Online" : "Error",
                StatusCode = (int)response.StatusCode
            };
        } catch (Exception ex) {
            return new { Status = "Offline", Error = ex.Message };
        }
    }
}