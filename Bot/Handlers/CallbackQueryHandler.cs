using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz.Bot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly CalculationHandlers _calculationHandlers;
        private readonly MessageHandlers _messageHandlers;

        public CallbackQueryHandler(ITelegramBotClient botClient, CalculationHandlers calculationHandlers, MessageHandlers messageHandlers)
        {
            _botClient = botClient;
            _calculationHandlers = calculationHandlers;
            _messageHandlers = messageHandlers;
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

                    // Добавляем новые case для обработки выбора типа процентов
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
                        await _botClient.SendMessage(chatId,
                            "You selected Floating Rate. Please enter the loan amount.");
                        state.Step = 2;
                        break;
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
