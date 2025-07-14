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
using TG_Fitz.Core;

namespace TelegramBot_Fitz.Bot {
    public class CalculationHandlers {
        private readonly ITelegramBotClient _botClient;
        private readonly TradeService _tradeService;

        public CalculationHandlers(ITelegramBotClient botClient, TradeService tradeService) {
            _botClient = botClient;
            _tradeService = tradeService;
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
            await _tradeService.SaveTradeAsync(chatId, state);

            state.Reset();
        }

        public async Task HandleFloatingRateCalculation(long chatId, UserState state) {
            var calculator = CalculatorFactory.GetCalculator(state.CalculationType);

            if (calculator is not IFloatingRateCalculator floatingCalculator) {
                await _botClient.SendMessage(chatId, "❌ Internal error: Expected floating rate calculator.");
                return;
            }

            var breakdown = floatingCalculator.GetInterestBreakdown(state);
            decimal totalInterest = breakdown.Sum(p => p.Interest);

            var sb = new StringBuilder();
            sb.AppendLine("📊 Floating Rate Calculation");
            sb.AppendLine($"🏢 Company: {state.CompanyName ?? "Untitled"}");
            sb.AppendLine($"💰 Loan Amount: {state.LoanAmount:F2} USD");
            sb.AppendLine($"📅 Duration: {state.LoanYears} years");
            sb.AppendLine($"🔁 Reset every {(int)state.FloatingRateResetPeriod} months\n");

            foreach (var period in breakdown) {
                sb.AppendLine($"📌 Period {period.PeriodNumber}: {period.Rate}% → {period.Interest:F2} USD");
            }

            sb.AppendLine($"\n💸 Total Interest: {totalInterest:F2} USD");

            await _botClient.SendMessage(chatId, sb.ToString());

            await _tradeService.SaveTradeAsync(chatId, state);

            state.Reset();
        }


        public async Task HandleOISCalculation(long chatId, UserState state, DayCountConvention dayCountConvention) {
            var calculator = CalculatorFactory.GetCalculator(state.CalculationType);

            if (calculator is OISCalculator oisCalculator) {
                var calculationResult = oisCalculator.CalculateOIS(state,dayCountConvention);
                var resultMessage = OISResultFormatter.FormatCalculationResult(calculationResult, state);
                await _botClient.SendMessage(chatId, resultMessage);
            } else {
                await _botClient.SendMessage(chatId, "❌ Error: Incorrect calculator type for OIS.");
            }

            state.Reset();
        }
    }
}
