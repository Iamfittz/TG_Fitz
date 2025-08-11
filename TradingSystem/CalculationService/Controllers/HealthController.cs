using Microsoft.AspNetCore.Mvc;

namespace CalculationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase {
    [HttpGet]
    public IActionResult Get() {
        var response = new {
            Status = "Healthy",
            Service = "Calculation Service",
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }
}