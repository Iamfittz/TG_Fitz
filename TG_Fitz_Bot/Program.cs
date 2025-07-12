using Microsoft.Extensions.Configuration;
using System;
using Telegram.Bot;
using TelegramBot_Fitz.Bot;
using Microsoft.Extensions.DependencyInjection;
using TG_Fitz.Data;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Reflection;
using Fitz.Core.Models;

namespace TelegramBot_Fitz {
    internal class Program {
        static async Task Main(string[] args) {
            var cliArgs = Environment.GetCommandLineArgs();
            if (cliArgs.Any(a => a.Contains("--ef-migrations"))) {
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
            if (string.IsNullOrEmpty(botToken)) {
                Console.WriteLine("ERROR: Bot token is missing!");
                throw new InvalidOperationException("Bot token is missing in configuration");
            }

            //DI
            var services = new ServiceCollection();
            services.AddSingleton(botToken);
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString)) {
                Console.WriteLine("Database connection string is missing!");
                throw new InvalidOperationException("Database connection string is missing in configuration");
            }
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));
            services.AddHttpClient<SofrService>(); // Регистрируем HttpClient для SofrService
            services.AddScoped<SofrService>(); // Регистрируем SofrService
            services.AddScoped<BotService>(sp =>
            {
                var token = sp.GetRequiredService<string>();
                var db = sp.GetRequiredService<AppDbContext>();
                var sofr = sp.GetRequiredService<SofrService>();
                var tradeService = sp.GetRequiredService<TradeService>();
                return new BotService(token, db, sofr, tradeService);
            });

            services.AddScoped<TradeService>();
            var serviceProvider = services.BuildServiceProvider();


            Console.WriteLine("Bot is running...");

            using var cts = new CancellationTokenSource();
            var scope = serviceProvider.CreateScope();
            var botService = scope.ServiceProvider.GetRequiredService<BotService>(); // Получаем BotService через DI
            bool isCancellationRequested = false;

            Console.CancelKeyPress += (sender, e) => {
                if (!isCancellationRequested) {
                    Console.WriteLine("Received shutdown signal (Ctrl+C). Stopping bot...");
                    e.Cancel = true;
                    isCancellationRequested = true;
                    cts.Cancel();
                }
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
                if (!isCancellationRequested) {
                    Console.WriteLine("Received SIGTERM. Stopping bot...");
                    isCancellationRequested = true;
                    cts.Cancel();
                }
            };

            try {
                botService.Start();
                await Task.Delay(Timeout.Infinite, cts.Token);
            } catch (TaskCanceledException) {
                Console.WriteLine("Bot is shutting down gracefully...");
                botService.Stop();
            } catch (Exception ex) {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                botService.Stop();
            } finally {
                Console.WriteLine("Bot has stopped.");
            }
        }
    }
}
