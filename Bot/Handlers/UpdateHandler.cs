using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Fitz.Data;

namespace TelegramBot_Fitz.Bot.Handlers
{
    public class UpdateHandler
    {
        private readonly ITelegramBotClient _botСlient;
        private readonly Dictionary<long, UserState> _userStates;
        private readonly MessageHandlers _messageHandlers;
        private readonly InputHandlers _inputHandlers;
        private readonly CallbackQueryHandler _callbackQueryHandler;
        public UpdateHandler(ITelegramBotClient botСlient, Dictionary<long, UserState> userStates,
            MessageHandlers messageHandlers, InputHandlers inputHandlers, CallbackQueryHandler callbackQueryHandler)
        {
            _botСlient = botСlient;
            _userStates = userStates;
            _messageHandlers = messageHandlers;
            _inputHandlers = inputHandlers;
            _callbackQueryHandler = callbackQueryHandler;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;
            long chatId = GetChatId(update);

            if (chatId == 0) return;

            // ДОБАВЛЯЕМ СОХРАНЕНИЕ ПОЛЬЗОВАТЕЛЯ В БД
            using (var db =new AppDbContext())
            {
                var existingUser = db.Users.FirstOrDefault(u => u.TG_ID == chatId);

                if (existingUser == null) // Если пользователя нет в базе, добавляем его
                {
                    var newUser = new TG_Fitz.Data.User 
                    { TG_ID = chatId,
                      Username = message?.From?.Username
                    };
                    db.Users.Add(newUser);
                    db.SaveChanges();
                    Console.WriteLine($" Новый пользователь добавлен в БД: {chatId}");
                }
                else
                {
                    existingUser.Username = message?.From?.Username;
                    db.SaveChanges();
                    Console.WriteLine($" Пользователь уже есть в БД: {chatId}");
                }
            }

            var userState = EnsureUserState(chatId);

            if (message?.Text != null)
            {
                var text = message.Text;

                if (text.StartsWith("/start") || text.StartsWith("/help"))
                {
                    await _messageHandlers.ShowWelcomeMessage(chatId);
                    return;
                }

                switch (userState.Step)
                {
                    case 2:
                        await _inputHandlers.HandleAmountInput(chatId, userState, text);
                        break;
                    case 3:
                        await _inputHandlers.HandleYearsInput(chatId, userState, text);
                        break;
                    case 4:
                        await _inputHandlers.HandleRateInput(chatId, userState, text);
                        break;
                    case 5:
                        await _inputHandlers.HandleSecondRateInput(chatId, userState, text);
                        break;
                }
            }

            if (callbackQuery != null && callbackQuery.Data != null)
            {
                await _callbackQueryHandler.HandleCallbackQuery(chatId, userState, callbackQuery.Data);
            }

        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error occurred: {exception.Message}");
            return Task.CompletedTask;
        }

        private long GetChatId(Update update)
        {
            return update.Message?.Chat?.Id
                ?? update.CallbackQuery?.Message?.Chat?.Id
                ?? 0; 
        }



        private UserState EnsureUserState(long chatId)
        {
            if (!_userStates.ContainsKey(chatId))
            {
                _userStates[chatId] = new UserState();
            }
            return _userStates[chatId];
        }

    }
}
