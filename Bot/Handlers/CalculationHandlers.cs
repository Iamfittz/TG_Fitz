using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_Fitz.Core;
using Fitz.Core.Enums;
using Fitz.Core.Factories;
using Fitz.Core.Interfaces;
using Fitz.Core.Strategies;
using Fitz.Core.States;
using Fitz.Core.Models;

namespace TelegramBot_Fitz.Bot {
    public class CalculationHandlers {
        private readonly ITelegramBotClient _botClient;

        public CalculationHandlers(ITelegramBotClient botClient) {
            _botClient = botClient;
        }

        public async Task HandleFixedRateCalculation(long chatId, UserState state) {
            var calculator = CalculatorFactory.GetCalculator(state.CalculationType);

            if (calculator is FixedRateLoanCalculator fixedRateCalculator) {
                var calculationResult = fixedRateCalculator.CalculateLoan(
                    state.LoanAmount,
                    state.YearlyRates,
                    state.InterestCalculationType
                );

                StringBuilder message = new StringBuilder();
                message.AppendLine($"📊 {state.InterestCalculationType} Interest Calculation\n");
                message.AppendLine($"Initial amount: {state.LoanAmount:F2} USD\n");

                foreach (var yearCalc in calculationResult.YearlyCalculations) {
                    message.AppendLine($"Year {yearCalc.Year}:");
                    message.AppendLine($"Rate: {yearCalc.Rate}%");
                    message.AppendLine($"Interest: {yearCalc.Interest:F2} USD");

                    if (state.InterestCalculationType == InterestCalculationType.Compound) {
                        message.AppendLine($"Accumulated amount: {yearCalc.AccumulatedAmount:F2} USD");
                    }
                    message.AppendLine();
                }

                message.AppendLine($"Total Interest: {calculationResult.TotalInterest:F2} USD");
                message.AppendLine($"Total Payment: {calculationResult.TotalPayment:F2} USD");

                var afterCalculation = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("📊 New Calculation", "NewCalculation"),
                            InlineKeyboardButton.WithCallbackData("🏠 Main Menu", "MainMenu") },
                    new[] { InlineKeyboardButton.WithCallbackData("❓ Help", "Help") }
                });

                await _botClient.SendMessage(
                    chatId,
                    message.ToString() + "\n\nWhat would you like to do next, anon?",
                    replyMarkup: afterCalculation
                );
            } else {
                await _botClient.SendMessage(chatId, "❌ Error: Incorrect calculator type for fixed rate.");
            }

            // Сохраняем трейд в базу
            var tradeService = new TradeService();
            await tradeService.SaveTradeAsync(chatId, state);

            state.Reset();
        }

        public async Task HandleFloatingRateCalculation(long chatId, UserState state) {
            var calculator = CalculatorFactory.GetCalculator(state.CalculationType);

            if (calculator is FloatingRateLoanCalculator floatingCalculator) {
                floatingCalculator.LoanAmount = state.LoanAmount;
                floatingCalculator.TotalYears = state.LoanYears;

                decimal totalInterest = floatingCalculator.CalculateTotalInterest(state);
                decimal totalPayment = floatingCalculator.CalculateTotalPayment(state);

                var sb = new StringBuilder();
                sb.AppendLine($"📊 Floating Rate Calculation");
                sb.AppendLine($"🏢 Company: {state.CompanyName ?? "Untitled"}");
                sb.AppendLine($"💰 Loan Amount: {state.LoanAmount:F2} USD");
                sb.AppendLine($"📅 Duration: {state.LoanYears} years");
                sb.AppendLine($"🔁 Reset every {(int)state.FloatingRateResetPeriod} months");
                sb.AppendLine();

                for (int i = 0; i < state.FloatingRates.Count; i++) {
                    var rate = state.FloatingRates[i];
                    sb.AppendLine($"📌 Period {i + 1}: {rate}%");
                }

                sb.AppendLine();
                sb.AppendLine($"💸 Total Interest: {totalInterest:F2} USD");
                sb.AppendLine($"💵 Total Payment: {totalPayment:F2} USD");

                await _botClient.SendMessage(chatId, sb.ToString());

                // ✅ сохранить сделку
                var tradeService = new TradeService();
                await tradeService.SaveTradeAsync(chatId, state);

                state.Reset();
            } else {
                await _botClient.SendMessage(chatId, "❌ Error: Incorrect calculator type for floating rate.");
            }
        }


        public async Task HandleOISCalculation(long chatId, UserState state) {
            var calculator = CalculatorFactory.GetCalculator(state.CalculationType);

            if (calculator is OISCalculator oisCalculator) {
                var calculationResult = oisCalculator.CalculateOIS(state);
                var resultMessage = oisCalculator.FormatCalculationResult(calculationResult, state);
                await _botClient.SendMessage(chatId, resultMessage);
            } else {
                await _botClient.SendMessage(chatId, "❌ Error: Incorrect calculator type for OIS.");
            }

            state.Reset();
        }
    }
}
