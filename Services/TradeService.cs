using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TG_Fitz.Data;
using Fitz.Core.Models;
using Fitz.Core.States;   

namespace Fitz.Core.Models;

public class TradeService {
    public async Task SaveTradeAsync(long chatId, UserState state, string? companyName = null) {
        using var db = new AppDbContext();

        var user = db.Users.FirstOrDefault(u => u.TG_ID == chatId);
        if (user == null) {
            Console.WriteLine($"❌ User not found for TG_ID: {chatId}");
            return;
        }

        var trade = new Trade {
            CompanyName = state.CompanyName ?? "—",
            LoanAmount = state.LoanAmount,
            Years = state.LoanYears,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };

        db.Trades.Add(trade);
        await db.SaveChangesAsync();
        Console.WriteLine($"✅ Trade saved for user {chatId}");
    }

}

