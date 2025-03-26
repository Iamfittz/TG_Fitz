using Microsoft.Extensions.Configuration;
using System;
using Telegram.Bot;
using TelegramBot_Fitz.Bot;
using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cliArgs =Environment.GetCommandLineArgs();
            if(cliArgs.Any(a=>a.Contains("ef")))
            {
                Console.WriteLine("⏳ EF Core operation detected. Bot will not start.");
                return;
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            Console.WriteLine($"Environment: {environment}");

            // Загружаем конфигурацию
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            // Проверяем, загружается ли botToken
            string? botToken = configuration["BotSettings:BotToken"];
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("ERROR: Bot token is missing!");
                throw new InvalidOperationException("Bot token is missing in configuration");
            }

            Console.WriteLine("Bot is running...");

            // Запускаем бота
            var botService = new BotService(botToken);
            botService.Start();

            while (true) Thread.Sleep(1000);
            //Console.ReadLine();
        }
    }
}
