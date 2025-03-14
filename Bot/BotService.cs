using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TelegramBot_Fitz.Core;
using TelegramBot_Fitz.Bot.Handlers;

namespace TelegramBot_Fitz.Bot
{
    public class BotService
    {
        private static BotService? _instance;
        private static readonly object _lock = new object();
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, UserState> _userStates;
        private readonly MessageHandlers _messageHandlers;
        private readonly CalculationHandlers _calculationHandlers;
        private readonly InputHandlers _inputHandlers;
        private readonly UpdateHandler _updateHandler;
        private readonly CallbackQueryHandler _callbackQueryHandler;

        public BotService(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userStates = new Dictionary<long, UserState>();

            var fixedCalculator = new FixedRateLoanCalculator();
            var floatingCalculator = new FloatingRateLoanCalculator();
            var oisCalculator = new OISCalculator();

            _messageHandlers = new MessageHandlers(_botClient);
            _calculationHandlers = new CalculationHandlers(_botClient);
            _inputHandlers = new InputHandlers(_botClient, _calculationHandlers);
            _callbackQueryHandler = new CallbackQueryHandler(_botClient, _calculationHandlers, _messageHandlers);
            _updateHandler = new UpdateHandler(_botClient, _userStates, _messageHandlers, _inputHandlers, _callbackQueryHandler);
        }

        //public static BotService GetInstance(string token)
        //{
        //    if (_instance == null)
        //    {
        //        lock (_lock)
        //        {
        //            if (_instance == null)
        //            {
        //                _instance = new BotService(token);
        //            }
        //        }
        //    }
        //    return _instance;
        //}

        public void Start()
        {
            _botClient.StartReceiving(_updateHandler.HandleUpdateAsync, _updateHandler.HandleErrorAsync);
        }
    }
}