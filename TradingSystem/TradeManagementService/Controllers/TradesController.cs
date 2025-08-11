using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeManagementService.Data;
using TradeManagementService.Models;
using CalculationService.Core.Enums;

namespace TradeManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradesController : ControllerBase {
    private readonly TradeDbContext _context;

    public TradesController(TradeDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Save new trade
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTrade([FromBody] CreateTradeRequest request) {
        try {
            // Find or create user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TelegramId == request.TelegramId);

            if (user == null) {
                user = new User {
                    TelegramId = request.TelegramId,
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Create trade with enum conversion
            var trade = new Trade {
                OriginalTradeId = request.OriginalTradeId ?? "",
                CompanyName = request.CompanyName,
                LoanAmount = request.LoanAmount,
                Currency = request.Currency ?? "USD",
                Years = request.Years,

                EffectiveDate = request.EffectiveDate,
                TerminationDate = request.TerminationDate,
                TradeDate = request.TradeDate,

                CalculationType = ParseCalculationType(request.CalculationType),
                InterestType = ParseInterestCalculationType(request.InterestType),

                FixedRate = request.FixedRate,
                FixedLegDayCount = ParseDayCountConvention(request.FixedLegDayCount),
                FixedLegPaymentFreq = request.FixedLegPaymentFreq,

                FloatingRateIndex = request.FloatingRateIndex,
                FloatingLegTenor = request.FloatingLegTenor,
                FloatingLegDayCount = ParseDayCountConvention(request.FloatingLegDayCount),
                FloatingLegPaymentFreq = request.FloatingLegPaymentFreq,

                BusinessDayConvention = ParseBusinessDayConvention(request.BusinessDayConvention),
                BusinessCenters = request.BusinessCenters,

                PayerParty = request.PayerParty,
                ReceiverParty = request.ReceiverParty,
                CounterpartyName = request.CounterpartyName,

                TotalInterest = request.TotalInterest,
                TotalPayment = request.TotalPayment,
                CalculationData = request.CalculationData,

                OriginalXmlContent = request.OriginalXmlContent,

                UserId = user.Id
            };

            _context.Trades.Add(trade);
            await _context.SaveChangesAsync();

            return Ok(new {
                Success = true,
                TradeId = trade.Id,
                OriginalTradeId = trade.OriginalTradeId,
                Message = "Trade saved successfully!"
            });
        } catch (Exception ex) {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get all user trades
    /// </summary>
    [HttpGet("user/{telegramId}")]
    public async Task<IActionResult> GetUserTrades(long telegramId) {
        try {
            var trades = await _context.Trades
                .Include(t => t.User)
                .Where(t => t.User.TelegramId == telegramId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new {
                    t.Id,
                    t.OriginalTradeId,
                    t.CompanyName,
                    t.LoanAmount,
                    t.Currency,
                    t.Years,
                    CalculationType = t.CalculationType.ToString(),
                    InterestType = t.InterestType.HasValue ? t.InterestType.ToString() : null,
                    t.TotalInterest,
                    t.TotalPayment,
                    t.EffectiveDate,
                    t.TerminationDate,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(new {
                Success = true,
                Trades = trades,
                Count = trades.Count
            });
        } catch (Exception ex) {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get specific trade
    /// </summary>
    [HttpGet("{tradeId}")]
    public async Task<IActionResult> GetTrade(int tradeId) {
        try {
            var trade = await _context.Trades
                .Include(t => t.User)
                .Where(t => t.Id == tradeId)
                .Select(t => new {
                    t.Id,
                    t.OriginalTradeId,
                    t.CompanyName,
                    t.LoanAmount,
                    t.Currency,
                    t.Years,
                    CalculationType = t.CalculationType.ToString(),
                    InterestType = t.InterestType.HasValue ? t.InterestType.ToString() : null,

                    t.FixedRate,
                    FixedLegDayCount = t.FixedLegDayCount.HasValue ? t.FixedLegDayCount.ToString() : null,
                    t.FixedLegPaymentFreq,

                    t.FloatingRateIndex,
                    t.FloatingLegTenor,
                    FloatingLegDayCount = t.FloatingLegDayCount.HasValue ? t.FloatingLegDayCount.ToString() : null,
                    t.FloatingLegPaymentFreq,

                    BusinessDayConvention = t.BusinessDayConvention.HasValue ? t.BusinessDayConvention.ToString() : null,
                    t.BusinessCenters,

                    t.PayerParty,
                    t.ReceiverParty,
                    t.CounterpartyName,

                    t.EffectiveDate,
                    t.TerminationDate,
                    t.TradeDate,

                    t.TotalInterest,
                    t.TotalPayment,
                    t.CalculationData,

                    t.CreatedAt,
                    t.UpdatedAt,

                    User = new {
                        t.User.Id,
                        t.User.TelegramId,
                        t.User.Username,
                        t.User.FirstName,
                        t.User.LastName
                    }
                })
                .FirstOrDefaultAsync();

            if (trade == null)
                return NotFound(new { Error = "Trade not found" });

            return Ok(new {
                Success = true,
                Trade = trade
            });
        } catch (Exception ex) {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // Helper methods for string to enum conversion
    private static CalculationType ParseCalculationType(string? value) {
        return value?.ToLower() switch {
            "fixedrate" => CalculationType.FixedRate,
            "floatingrate" => CalculationType.FloatingRate,
            "ois" => CalculationType.OIS,
            "interestrateswap" => CalculationType.FixedRate,
            _ => CalculationType.None
        };
    }

    private static InterestCalculationType? ParseInterestCalculationType(string? value) {
        return value?.ToLower() switch {
            "simple" => InterestCalculationType.Simple,
            "compound" => InterestCalculationType.Compound,
            _ => null
        };
    }

    private static DayCountConvention? ParseDayCountConvention(string? value) {
        return value?.ToUpper() switch {
            "ACT/360" => DayCountConvention.Actual360,
            "ACT/365" => DayCountConvention.Actual365,
            "30/360" => DayCountConvention.Thirty360,
            "ACT/ACT" => DayCountConvention.ActualActual,
            _ => null
        };
    }

    private static BusinessDayConvention? ParseBusinessDayConvention(string? value) {
        return value?.ToUpper() switch {
            "FOLLOWING" => BusinessDayConvention.FOLLOWING,
            "MODFOLLOWING" => BusinessDayConvention.MODFOLLOWING,
            "PRECEDING" => BusinessDayConvention.PRECEDING,
            "FRN" => BusinessDayConvention.FRN,
            "MODPRECEDING" => BusinessDayConvention.MODPRECEDING,
            "NEAREST" => BusinessDayConvention.NEAREST,
            "NONE" => BusinessDayConvention.NONE,
            "NOTAPPLICABLE" => BusinessDayConvention.NotApplicable,
            _ => null
        };
    }
}

// Model for creating trade with all new fields
public class CreateTradeRequest {
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string? OriginalTradeId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public string? Currency { get; set; }
    public int Years { get; set; }

    public DateTime? EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public DateTime? TradeDate { get; set; }

    public string CalculationType { get; set; } = string.Empty;
    public string? InterestType { get; set; }

    public decimal? FixedRate { get; set; }
    public string? FixedLegDayCount { get; set; }
    public string? FixedLegPaymentFreq { get; set; }

    public string? FloatingRateIndex { get; set; }
    public string? FloatingLegTenor { get; set; }
    public string? FloatingLegDayCount { get; set; }
    public string? FloatingLegPaymentFreq { get; set; }

    public string? BusinessDayConvention { get; set; }
    public string? BusinessCenters { get; set; }

    public string? PayerParty { get; set; }
    public string? ReceiverParty { get; set; }
    public string? CounterpartyName { get; set; }

    public decimal TotalInterest { get; set; }
    public decimal TotalPayment { get; set; }
    public string? CalculationData { get; set; }

    public string? OriginalXmlContent { get; set; }
}