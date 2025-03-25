using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using TG_Fitz.Data;

namespace TelegramBot_Fitz.Bot
{
    public class MessageHandlers
    {
        private readonly ITelegramBotClient _botClient;

        public MessageHandlers(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task ShowWelcomeMessage(long chatId)
        {
            var welcomeMessage =
                "👋 Welcome to the Derivatives Calculator Bot!\n\n" +
                "I'm your personal assistant for calculating " +
                "various derivative instruments. " +
                "I can help you evaluate different types " +
                "of derivatives and their rates.\n\n" +
                "Before we begin, please select the derivative " +
                "instrument you'd like to calculate:";

            var inlineKeybord = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("📊 IRS Fixed Float", "IRS_Fixed_Float") },
                new[] { InlineKeyboardButton.WithCallbackData("💹 IRS OIS", "IRS_OIS") }
            });

            await _botClient.SendMessage(
                chatId,
                welcomeMessage,
                replyMarkup: inlineKeybord
            );
        }

        public async Task ShowRateTypeSelection(long chatId)
        {
            await _botClient.SendMessage(chatId,
                "You've selected Interest Rate Swap (IRS) Fixed Float.\n" +
                "Please choose the type of rate calculation:");

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new [] { InlineKeyboardButton.WithCallbackData("📈 Fixed Rate", "FixedRate") },
                new [] { InlineKeyboardButton.WithCallbackData("📊 Floating Rate", "FloatingRate") }
            });

            await _botClient.SendMessage(chatId,
                "Select your preferred rate type:",
                replyMarkup: inlineKeyboard);
        }

        public async Task ShowTradeHistory(long chatId)
        {
            using var db = new AppDbContext();
            var user = db.Users
                .Where(u => u.TG_ID == chatId)
                .Select(u => new
                {
                    u.Trades
                })
                .FirstOrDefault();

            if (user == null || user.Trades == null || !user.Trades.Any())
            {
                await _botClient.SendMessage(chatId, "❌ У вас пока нет сохранённых трейдов.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("📄 Ваши сделки:\n");

            foreach (var trade in user.Trades.OrderByDescending(t => t.CreatedAt))
            {
                sb.AppendLine($"🏢 {trade.CompanyName ?? "Без названия"}");
                sb.AppendLine($"💰 Сумма: {trade.LoanAmount} USD");
                sb.AppendLine($"📅 Срок: {trade.Years} лет");
                sb.AppendLine($"🕓 Дата: {trade.CreatedAt:yyyy-MM-dd}");
                sb.AppendLine("———");
            }

            await _botClient.SendMessage(chatId, sb.ToString());
        }


    }
}
