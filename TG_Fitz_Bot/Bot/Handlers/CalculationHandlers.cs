using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Fitz.Services;
using CalculationService.Core.States;

namespace TelegramBot_Fitz.Bot {
    public class CalculationHandlers {
        private readonly ITelegramBotClient _botClient;
        private readonly ApiGatewayService _apiGatewayService;

        public CalculationHandlers(ITelegramBotClient botClient, ApiGatewayService apiGatewayService) {
            _botClient = botClient;
            _apiGatewayService = apiGatewayService;
        }

        /// <summary>
        /// 🚀 Универсальный метод для всех типов расчетов через Gateway
        /// </summary>
        public async Task HandleCalculationViaGateway(long chatId, object userState, string calculationType) {
            try {
                // Преобразуем UserState в запрос для Gateway
                var request = ConvertToGatewayRequest(chatId, userState, calculationType);

                // Вызываем Gateway (расчет + сохранение)
                var response = await _apiGatewayService.CalculateAndSaveAsync(request);

                if (response?.Success == true) {
                    // Форматируем и отправляем результат
                    var message = FormatCalculationResult(response, calculationType);

                    var afterCalculation = new InlineKeyboardMarkup(new[]
                    {
                        new[] {
                            InlineKeyboardButton.WithCallbackData("📊 New Calculation", "NewCalculation"),
                            InlineKeyboardButton.WithCallbackData("🏠 Main Menu", "MainMenu")
                        },
                        new[] { InlineKeyboardButton.WithCallbackData("❓ Help", "Help") }
                    });

                    await _botClient.SendMessage(chatId, message, replyMarkup: afterCalculation);
                } else {
                    await _botClient.SendMessage(chatId, "❌ Calculation failed. Please try again.");
                }
            } catch (Exception ex) {
                await _botClient.SendMessage(chatId, $"❌ Error: {ex.Message}");
            }
        }

        private ApiGatewayRequest ConvertToGatewayRequest(long chatId, object userStateObj, string calculationType) {
            // Приводим object к UserState
            if (userStateObj is not CalculationService.Core.States.UserState userState)
                throw new ArgumentException("Invalid UserState");

            return new ApiGatewayRequest {
                TelegramId = chatId,
                CalculationType = calculationType,
                CompanyName = userState.CompanyName ?? "Unknown Company",
                LoanAmount = userState.LoanAmount,
                Years = userState.LoanYears,
                InterestType = userState.InterestCalculationType.ToString(),

                // FixedRate данные
                YearlyRates = userState.YearlyRates,

                // FloatingRate данные
                FloatingRates = userState.FloatingRates?.ToArray(),
                ResetPeriod = (int)userState.FloatingRateResetPeriod,

                // OIS данные
                OvernightRate = userState.FirstRate,
                Days = userState.Days,
                DayCountConvention = (int)userState.DayCountConvention
            };
        }

        private string FormatCalculationResult(ApiGatewayResponse response, string calculationType) {
            var sb = new StringBuilder();
            sb.AppendLine($"✅ {calculationType} Calculation Completed!");
            sb.AppendLine();
            sb.AppendLine("📊 Results:");

            // Здесь нужно будет парсить response.Calculation 
            // и красиво форматировать результат

            sb.AppendLine($"🎉 {response.Message}");

            return sb.ToString();
        }

        // Оставляем старые методы как заглушки пока не переделаем весь бот
        public async Task HandleFixedRateCalculation(long chatId, object state) {
            await HandleCalculationViaGateway(chatId, state, "FixedRate");
        }

        public async Task HandleFloatingRateCalculation(long chatId, object state) {
            await HandleCalculationViaGateway(chatId, state, "FloatingRate");
        }

        public async Task HandleOISCalculation(long chatId, object state, object dayCountConvention) {
            await HandleCalculationViaGateway(chatId, state, "OIS");
        }
    }
}