using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_Fitz.Core;
using TG_Fitz.Core;
using TG_Fitz.Data;

namespace TelegramBot_Fitz.Bot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly CalculationHandlers _calculationHandlers;
        private readonly MessageHandlers _messageHandlers;
        private readonly AppDbContext _dbContext;

        public CallbackQueryHandler(ITelegramBotClient botClient, CalculationHandlers calculationHandlers, MessageHandlers messageHandlers, AppDbContext dbContext)
        {
            _botClient = botClient;
            _calculationHandlers = calculationHandlers;
            _messageHandlers = messageHandlers;
            _dbContext = dbContext;
        }

        public async Task HandleCallbackQuery(long chatId, UserState state, string callbackData)
        {
            if(callbackData == "NewCalculation")
            {
                state.Step = 1;
                await _botClient.SendMessage(chatId, "🏢 Please enter your company name (eg Apple, Tesla, etc.):");
                return;
            }
            
            if (callbackData.StartsWith("SameRate_"))
            {
                int nextYear = int.Parse(callbackData.Split('_')[1]);
                state.YearlyRates[nextYear - 1] = state.YearlyRates[nextYear - 2]; // Копируем предыдущую ставку
                state.CurrentYear = nextYear;

                if (nextYear < state.LoanYears)
                {
                    // Если есть еще годы, спрашиваем про следующий
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Use same rate", $"SameRate_{nextYear + 1}"),
                    InlineKeyboardButton.WithCallbackData("Enter new rate", $"NewRate_{nextYear + 1}")
                }
            });

                    await _botClient.SendMessage(chatId,
                        $"Rate for year {nextYear} is set to {state.YearlyRates[nextYear - 1]}%.\n" +
                        $"What about year {nextYear + 1}?",
                        replyMarkup: keyboard);
                }
                else
                {
                    // Если это был последний год, делаем расчет
                    await _calculationHandlers.HandleFixedRateCalculation(chatId, state);
                }
            }
            else if (callbackData.StartsWith("NewRate_"))
            {
                int nextYear = int.Parse(callbackData.Split('_')[1]);
                state.CurrentYear = nextYear;
                await _botClient.SendMessage(chatId,
                    $"Please enter the interest rate for year {nextYear} (e.g., 4 for 4%):");
            }

            else if (callbackData.StartsWith("ShowTrade_"))
            {
                if (int.TryParse(callbackData.Split('_')[1], out int tradeId))
                {
                    var trade = _dbContext.Trades.FirstOrDefault(t => t.Id == tradeId && t.User.TG_ID == chatId);

                    if (trade != null)
                    {
                        var company = string.IsNullOrWhiteSpace(trade.CompanyName) ? "Untitled" : trade.CompanyName;

                        var detailMessage = $"🏢 Company: {company}\n" +
                                            $"💰 Loan: {trade.LoanAmount} USD\n" +
                                            $"📅 Duration: {trade.Years} years\n" +
                                            $"🕓 Date: {trade.CreatedAt:yyyy-MM-dd}";

                        await _botClient.SendMessage(chatId, detailMessage);
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId, "❌ Trade not found.");
                    }
                }
                else
                {
                    await _botClient.SendMessage(chatId, "⚠️ Invalid trade ID.");
                }

                return; // Важно: выходим, чтобы не провалиться в switch ниже
            }



            else
            {
                switch (callbackData)
                {
                    case "IRS_Fixed_Float":
                        await _messageHandlers.ShowRateTypeSelection(chatId);
                        state.Step = 1;
                        break;
                    case "IRS_OIS":
                        state.CalculationType = CalculationType.OIS;
                        await _botClient.SendMessage(chatId,
                            "You've selected OIS (Overnight Index Swap).\n" +
                            "Please enter the notional amount:");
                        state.Step = 2;
                        break;
                    case "FixedRate":
                        state.CalculationType = CalculationType.FixedRate;

                        // Создаем клавиатуру для выбора типа процентов
                        var interestTypeKeyboard = new InlineKeyboardMarkup(new[]
                        {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📊 Simple Interest", "SimpleInterest"),
                    InlineKeyboardButton.WithCallbackData("📈 Compound Interest", "CompoundInterest")
                }
            });
                        await _botClient.SendMessage(chatId,
                    "Please select interest calculation method:\n\n" +
                    "📊 Simple Interest: interest is calculated on the initial principal only\n" +
                    "📈 Compound Interest: interest is calculated on the accumulated amount",
                    replyMarkup: interestTypeKeyboard);
                        break;
                    case "SimpleInterest":
                        state.InterestCalculationType = InterestCalculationType.Simple;
                        await _botClient.SendMessage(chatId,
                            "You selected Simple Interest calculation.\n" +
                            "Please enter the loan amount.");
                        state.Step = 2;
                        break;
                    case "CompoundInterest":
                        state.InterestCalculationType = InterestCalculationType.Compound;
                        await _botClient.SendMessage(chatId,
                            "You selected Compound Interest calculation.\n" +
                            "Please enter the loan amount.");
                        state.Step = 2;
                        break;
                    case "FloatingRate":
                        state.CalculationType = CalculationType.FloatingRate;

                        var resetKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[] {
                                InlineKeyboardButton.WithCallbackData("🔁 Every 1 month", "Reset_1m"),
                                InlineKeyboardButton.WithCallbackData("🔁 Every 3 months", "Reset_3m")
                                },
                            new[] {
                                InlineKeyboardButton.WithCallbackData("🔁 Every 6 months", "Reset_6m"),
                                InlineKeyboardButton.WithCallbackData("🔁 Every 12 months", "Reset_12m")
                                }
                        });

                        await _botClient.SendMessage(chatId,
                            "You selected Floating Rate.\n" +
                            " How often should the rate reset?",
                            replyMarkup: resetKeyboard);
                        return;

                    case "Reset_1m":
                        state.FloatingRateResetPeriod = FloatingRateResetPeriod.OneMonth;
                        await _botClient.SendMessage(chatId,
                            "🔁 Rate will reset every 1 month.\n" +
                            "💰 Please enter the **loan amount** (e.g., 100000):");
                        state.Step = 2;
                        return;

                    case "Reset_3m":
                        state.FloatingRateResetPeriod = FloatingRateResetPeriod.ThreeMonth;
                        await _botClient.SendMessage(chatId,
                            "🔁 Rate will reset every 3 months.\n" +
                            "💰 Please enter the **loan amount** (e.g., 100000):");
                        state.Step = 2;
                        return;

                    case "Reset_6m":
                        state.FloatingRateResetPeriod = FloatingRateResetPeriod.SixMonth;
                        await _botClient.SendMessage(chatId,
                            "🔁 Rate will reset every 6 months.\n" +
                            "💰 Please enter the **loan amount** (e.g., 100000):");
                        state.Step = 2;
                        return;

                    case "Reset_12m":
                        state.FloatingRateResetPeriod = FloatingRateResetPeriod.OneYear;
                        await _botClient.SendMessage(chatId,
                            "🔁 Rate will reset every 12 months.\n" +
                            "💰 Please enter the **loan amount** (e.g., 100000):");
                        state.Step = 2;
                        return;
                    case "NewCalculation":
                        await _messageHandlers.ShowRateTypeSelection(chatId);
                        state.Step = 1;
                        break;

                    case "MainMenu":
                        await _messageHandlers.ShowWelcomeMessage(chatId);
                        state.Reset();
                        break;
                    case "ShowHistory":
                        await _messageHandlers.ShowTradeHistory(chatId);
                        break;

                    case "Help":
                        var helpMessage =
                            "📌 Available commands:\n\n" +
                            "/start - Start new calculation\n" +
                            "/help - Show this help message\n\n" +
                            "💡 Tips:\n" +
                            "• You can calculate fixed or floating rates\n" +
                            "• For fixed rates, you can set different rates for each year\n" +
                            "• All amounts should be positive numbers\n\n" +
                            "Need more help? Feel free to start a new calculation!";

                        var returnKeyboard = new InlineKeyboardMarkup(new[]
                        {
                new[] { InlineKeyboardButton.WithCallbackData("🔙 Back to Main Menu", "MainMenu") }
            });

                        await _botClient.SendMessage(chatId, helpMessage, replyMarkup: returnKeyboard);
                        break;
                }
            }
        }
    }
}
