using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_Fitz.Bot
{
    public class InputHandlers
    {
        private readonly ITelegramBotClient _botClient;
        private readonly CalculationHandlers _calculationHandlers;

        public InputHandlers(ITelegramBotClient botClient, CalculationHandlers calculationHandlers)
        {
            _botClient = botClient;
            _calculationHandlers = calculationHandlers;
        }

        public async Task HandleAmountInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal amount) && amount > 0)
            {
                state.LoanAmount = amount;
                if (state.CalculationType == CalculationType.OIS)
                {
                    await _botClient.SendMessage(chatId, "Please enter the number of days for OIS calculation:");
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Please enter the number of years:");
                }
                state.Step = 3;
            }
            else
            {
                await _botClient.SendMessage(chatId, "Please enter a valid positive amount.");
            }
        }

        public async Task HandleYearsInput(long chatId, UserState state, string input)
        {
            if (state.CalculationType == CalculationType.OIS)
            {
                if (int.TryParse(input, out int days) && days > 0)
                {
                    state.Days = days;
                    await _botClient.SendMessage(chatId,
                        "Please enter the overnight interest rate (e.g., 3.5 for 3.5%):");
                    state.Step = 4;
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Please enter a valid number of days.");
                }
            }
            else
            {
                if (int.TryParse(input, out int years) && years > 0)
                {
                    state.LoanYears = years;
                    state.InitilizeYearlyRates();

                    if (state.CalculationType == CalculationType.FixedRate)
                    {
                        state.LoanYears = years;
                        state.InitilizeYearlyRates();

                        await _botClient.SendMessage(chatId,
                            "Please enter the interest rate for year 1 (e.g., 4 for 4%).");
                        state.CurrentYear = 1;
                        state.Step = 4;
                    }
                    else
                    {
                        await _botClient.SendMessage(chatId,
                            "You've entered a floating rate calculation.\n" +
                            "Now please enter the interest rate for the first 6-month period:");
                    }
                    state.Step = 4;
                }
                else
                {
                    await _botClient.SendMessage(chatId, "Please enter a valid number of years.");
                }
            }
        }


        public async Task HandleRateInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal rate) && rate > 0)
            {
                if (state.CalculationType == CalculationType.FixedRate)
                {
                    // Сохраняем ставку для текущего года
                    state.YearlyRates[state.CurrentYear - 1] = rate;

                    if (state.CurrentYear < state.LoanYears)
                    {
                        // Если это не последний год, спрашиваем про следующий
                        var keyboard = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Use same rate", $"SameRate_{state.CurrentYear + 1}"),
                        InlineKeyboardButton.WithCallbackData("Enter new rate", $"NewRate_{state.CurrentYear + 1}")
                    }
                });

                        await _botClient.SendMessage(chatId,
                            $"Rate for year {state.CurrentYear} is set to {rate}%.\n" +
                            $"What about year {state.CurrentYear + 1}?",
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        // Если это последний год, делаем расчет
                        await _calculationHandlers.HandleFixedRateCalculation(chatId, state);
                    }
                }
                else if (state.CalculationType == CalculationType.FloatingRate)
                {
                    // Существующая логика для Floating Rate
                    state.FirstRate = rate;
                    await _botClient.SendMessage(chatId,
                        $"First 6-month period rate is set to {rate}%.\n" +
                        "Please enter the interest rate for the second 6-month period:");
                    state.Step = 5;
                }
                else if (state.CalculationType == CalculationType.OIS)
                {
                    // Существующая логика для OIS
                    state.FirstRate = rate;
                    await _calculationHandlers.HandleOISCalculation(chatId, state);
                }
            }
            else
            {
                var errorMessage = state.CalculationType == CalculationType.FixedRate
                    ? $"Please enter a valid interest rate for year {state.CurrentYear}."
                    : state.CalculationType == CalculationType.FloatingRate
                    ? "Please enter a valid interest rate for the first 6-month period."
                    : state.CalculationType == CalculationType.OIS
                    ? "Please enter a valid overnight interest rate."
                    : "Please enter a valid rate.";

                await _botClient.SendMessage(chatId, errorMessage);
            }
        }
        public async Task HandleSecondRateInput(long chatId, UserState state, string input)
        {
            if (decimal.TryParse(input, out decimal rate) && rate > 0)
            {
                state.SecondRate = rate;
                await _calculationHandlers.HandleFloatingRateCalculation(chatId, state);
            }
            else
            {
                await _botClient.SendMessage(chatId, "Please enter a valid interest rate for the second period.");
            }
        }
    }
}
