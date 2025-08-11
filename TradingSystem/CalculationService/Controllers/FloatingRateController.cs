using Microsoft.AspNetCore.Mvc;
using CalculationService.Core.Enums;
using CalculationService.Core.Calculators;
using CalculationService.Core.States;

namespace CalculationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FloatingRateController : ControllerBase {
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] FloatingRateRequest request) {
        try {
            var calculator = new FloatingRateLoanCalculator();

            // Создаем состояние как в боте
            var state = new UserState {
                LoanAmount = request.LoanAmount,
                LoanYears = request.Years,
                FloatingRates = request.Rates.ToList(),
                FloatingRateResetPeriod = request.ResetPeriod
            };

            var breakdown = calculator.GetInterestBreakdown(state);
            var totalInterest = breakdown.Sum(p => p.Interest);

            var response = new {
                Success = true,
                Data = new {
                    LoanAmount = request.LoanAmount,
                    TotalInterest = totalInterest,
                    Years = request.Years,
                    ResetPeriod = request.ResetPeriod.ToString(),
                    PeriodBreakdown = breakdown,
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

public class FloatingRateRequest {
    public decimal LoanAmount { get; set; }
    public int Years { get; set; }
    public decimal[] Rates { get; set; } = Array.Empty<decimal>();
    public FloatingRateResetPeriod ResetPeriod { get; set; }
    public string? CompanyName { get; set; }
}