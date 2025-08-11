using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeManagementService.Data;
using TradeManagementService.Models;

namespace TradeManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradesController : ControllerBase {
    private readonly TradeDbContext _context;

    public TradesController(TradeDbContext context) {
        _context = context;
    }

    /// <summary>
    /// 💾 Сохранить новый трейд
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTrade([FromBody] CreateTradeRequest request) {
        try {
            // 👤 Найти или создать пользователя
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

            // 💼 Создать трейд
            var trade = new Trade {
                CompanyName = request.CompanyName,
                LoanAmount = request.LoanAmount,
                Years = request.Years,
                CalculationType = request.CalculationType,
                InterestType = request.InterestType,
                TotalInterest = request.TotalInterest,
                TotalPayment = request.TotalPayment,
                CalculationData = request.CalculationData,
                UserId = user.Id
            };

            _context.Trades.Add(trade);
            await _context.SaveChangesAsync();

            return Ok(new {
                Success = true,
                TradeId = trade.Id,
                Message = "Trade saved successfully!"
            });
        } catch (Exception ex) {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 📋 Получить все трейды пользователя
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
                    t.CompanyName,
                    t.LoanAmount,
                    t.Years,
                    t.CalculationType,
                    t.TotalInterest,
                    t.TotalPayment,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(new {
                Success = true,
                Trades = trades
            });
        } catch (Exception ex) {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 🔍 Получить конкретный трейд
    /// </summary>
    [HttpGet("{tradeId}")]
    public async Task<IActionResult> GetTrade(int tradeId) {
        try {
            var trade = await _context.Trades
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == tradeId);

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
}

// 📝 Модель для создания трейда
public class CreateTradeRequest {
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public int Years { get; set; }
    public string CalculationType { get; set; } = string.Empty;
    public string? InterestType { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalPayment { get; set; }
    public string? CalculationData { get; set; }
}