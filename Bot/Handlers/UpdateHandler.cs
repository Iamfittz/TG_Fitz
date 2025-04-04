﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TG_Fitz.Bot.Handlers;
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
        private readonly SofrHandlers _sofrHandlers;
        public UpdateHandler(
            ITelegramBotClient botСlient, 
            Dictionary<long, UserState> userStates,
            MessageHandlers messageHandlers, 
            InputHandlers inputHandlers, 
            CallbackQueryHandler callbackQueryHandler, 
            SofrHandlers sofrHandlers)
        {
            _botСlient = botСlient;
            _userStates = userStates;
            _messageHandlers = messageHandlers;
            _inputHandlers = inputHandlers;
            _callbackQueryHandler = callbackQueryHandler;
            _sofrHandlers = sofrHandlers;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;
            long chatId = GetChatId(update);
            bool userAlreadyExist;

            if (chatId == 0) return;

            // ДОБАВЛЯЕМ СОХРАНЕНИЕ ПОЛЬЗОВАТЕЛЯ В БД
            using (var db = new AppDbContext())
            {
                var existingUser = db.Users.FirstOrDefault(u => u.TG_ID == chatId);
                userAlreadyExist = existingUser != null;
                if (!userAlreadyExist) // Если пользователя нет в базе, добавляем его
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

                if (userAlreadyExist && message?.Text == "/start")
                {
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[] {
                        InlineKeyboardButton.WithCallbackData("➕ New Trade", "NewCalculation"),
                        InlineKeyboardButton.WithCallbackData("📄 View history", "ShowHistory")
                              }
                    });

                    await _botСlient.SendMessage(chatId,
                        "👋 Welcome to the Derivatives Calculator Bot!\n\n" +
                        "I'm your personal assistant for calculating " +
                        "various derivative instruments. " +
                        "I can help you evaluate different types " +
                        "of derivatives and their rates.\n\n" +
                        "Before we begin, please choose " ,
                        replyMarkup: keyboard);

                    return;
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

                if (text == "/sofr")
                {
                    await _sofrHandlers.HandleSofrCommand(chatId, _botСlient);
                    return;
                }

                switch (userState.Step)
                {
                    case 1:
                        await _inputHandlers.HandleCompanyNameInput(chatId, userState, text);
                        break;
                    case 2:
                        await _inputHandlers.HandleAmountInput(chatId, userState, text);
                        break;
                    case 3:
                        await _inputHandlers.HandleYearsInput(chatId, userState, text);
                        break;
                    case 4:
                        if (userState.CalculationType == CalculationType.FloatingRate)
                            await _inputHandlers.HandleFloatingRateInput(chatId, userState, text);
                        else
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
