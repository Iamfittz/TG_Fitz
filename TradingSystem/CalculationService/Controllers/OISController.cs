using Microsoft.AspNetCore.Mvc;
using CalculationService.Core.Enums;
using CalculationService.Core.States;
using CalculationService.Core.Calculators;

namespace CalculationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OISController : ControllerBase {
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] OISRequest request) {
        try {
            var calculator = new OISCalculator();

            var state = new UserState {
                LoanAmount = request.NotionalAmount,
                FirstRate = request.OvernightRate,
                Days = request.Days
            };

            var result = calculator.CalculateOIS(state, request.DayCountConvention);

            var response = new {
                Success = true,
                Data = new {
                    NotionalAmount = request.NotionalAmount,
                    OvernightRate = request.OvernightRate,
                    Days = request.Days,
                    DayCountConvention = request.DayCountConvention.ToString(),
                    DailyRate = result.DailyRate,
                    TotalInterest = result.TotalInterest,
                    TotalPayment = result.TotalPayment,
                    CompanyName = request.CompanyName
                },
                CalculatedAt = DateTime.UtcNow
            };

            return Ok(response);
        } catch (Exception ex) {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

public class OISRequest {
    public decimal NotionalAmount { get; set; }
    public decimal OvernightRate { get; set; }
    public int Days { get; set; }
    public DayCountConvention DayCountConvention { get; set; }
    public string? CompanyName { get; set; }
}