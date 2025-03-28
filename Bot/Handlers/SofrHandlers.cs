using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TG_Fitz.Data;


namespace TG_Fitz.Bot.Handlers
{
    public class SofrHandlers
    {
        private readonly SofrService? _sofrService;

        public SofrHandlers(SofrService sofrService)
        {
            _sofrService = sofrService;
        }

        public async Task HandleSofrCommand(long chatId, ITelegramBotClient botClient)
        {
            string sofrText = await _sofrService!.GetLatestSofrAsync();
            await botClient.SendMessage(chatId, sofrText);
        }
    }
}
