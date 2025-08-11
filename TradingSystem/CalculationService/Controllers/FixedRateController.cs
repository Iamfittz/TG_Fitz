using Microsoft.AspNetCore.Mvc;
using CalculationService.Core.Enums;
using CalculationService.Core.Models;
using CalculationService.Core.Calculators;

namespace CalculationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixedRateController : ControllerBase {
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] FixedRateRequest request) {
        try {
            // Используем настоящий калькулятор из твоего перенесенного кода
            var calculator = new FixedRateLoanCalculator();

            var result = calculator.CalculateLoan(
                request.LoanAmount,
                request.YearlyRates,
                request.CalculationType
            );

            var response = new {
                Success = true,
                Data = new {
                    LoanAmount = request.LoanAmount,
                    TotalInterest = result.TotalInterest,
                    TotalPayment = result.TotalPayment,
                    CalculationType = request.CalculationType.ToString(),
                    YearlyBreakdown = result.YearlyCalculations,
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

// Модель запроса
public class FixedRateRequest {
    public decimal LoanAmount { get; set; }
    public decimal[] YearlyRates { get; set; } = Array.Empty<decimal>();
    public InterestCalculationType CalculationType { get; set; }
    public string? CompanyName { get; set; }
}