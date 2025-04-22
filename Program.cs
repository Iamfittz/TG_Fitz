using Microsoft.Extensions.Configuration;
using System;
using Telegram.Bot;
using TelegramBot_Fitz.Bot;
using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cliArgs =Environment.GetCommandLineArgs();
            if(cliArgs.Any(a=>a.Contains("--ef-migrations")))
            {
                Console.WriteLine("⏳ EF Core operation detected. Bot will not start.");
                return;
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            Console.WriteLine($"Environment: {environment}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            string? botToken = configuration["BotSettings:BotToken"];
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("ERROR: Bot token is missing!");
                throw new InvalidOperationException("Bot token is missing in configuration");
            }

            Console.WriteLine("Bot is running...");

            using var cts = new CancellationTokenSource();
            var botService = new BotService(botToken);
            bool isCancellationRequested = false;

            Console.CancelKeyPress += (sender, e) =>
            {
                if (!isCancellationRequested)
                {
                    Console.WriteLine("Received shutdown signal (Ctrl+C). Stopping bot...");
                    e.Cancel = true;
                    isCancellationRequested = true;
                    cts.Cancel();
                }
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                if (!isCancellationRequested)
                {
                    Console.WriteLine("Received SIGTERM. Stopping bot...");
                    isCancellationRequested = true;
                    cts.Cancel();
                }
            };

            try
            {
                botService.Start();
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Bot is shutting down gracefully...");
                botService.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                botService.Stop();
            }
            finally
            {
                // Здесь можно добавить финальную очистку, которая сработает в 100% случаях
                Console.WriteLine("Bot has stopped.");
            }
        }
    }
}
