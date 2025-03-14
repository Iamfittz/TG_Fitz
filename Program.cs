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
            // Определяем корневую папку проекта
            var currentDir = Directory.GetCurrentDirectory();
            var projectRoot = currentDir;

            Console.WriteLine($"Using base path: {projectRoot}");

            // Проверяем, существует ли appsettings.json
            string configPath = Path.Combine(projectRoot, "appsettings.json");
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"ERROR: Configuration file {configPath} not found!");
                throw new FileNotFoundException($"Configuration file {configPath} not found!");
            }

            // Загружаем конфигурацию
            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectRoot)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Проверяем, загружается ли botToken
            string? botToken = configuration["BotSettings:BotToken"];
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("ERROR: Bot token is null or empty!");
                throw new InvalidOperationException("Bot token is missing in appsettings.json");
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
