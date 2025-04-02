using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TelegramBot_Fitz.Core;
using TelegramBot_Fitz.Bot.Handlers;
using TG_Fitz.Data;
using TG_Fitz.Bot.Handlers;

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
        private readonly SofrHandlers _sofrHandlers;

        public BotService(string token)
        {
            _botClient = new TelegramBotClient(token);
            _userStates = new Dictionary<long, UserState>();
            
            var dbContext = new AppDbContext();
            var fixedCalculator = new FixedRateLoanCalculator();
            var floatingCalculator = new FloatingRateLoanCalculator();
            var oisCalculator = new OISCalculator();
            var sofrSeriсe = new SofrService(new HttpClient(),dbContext);
            var sofrHandlers = new SofrHandlers(sofrSeriсe);

            _messageHandlers = new MessageHandlers(_botClient);
            _calculationHandlers = new CalculationHandlers(_botClient);
            _inputHandlers = new InputHandlers(_botClient, _calculationHandlers);
            _callbackQueryHandler = new CallbackQueryHandler(_botClient, _calculationHandlers, _messageHandlers);
            _sofrHandlers = sofrHandlers;
            _updateHandler = new UpdateHandler(
                _botClient, 
                _userStates, 
                _messageHandlers, 
                _inputHandlers, 
                _callbackQueryHandler, 
                _sofrHandlers);
        }

        public void Start()
        {
            _botClient.StartReceiving(_updateHandler.HandleUpdateAsync, _updateHandler.HandleErrorAsync);
        }
    }
}