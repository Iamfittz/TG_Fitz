using Microsoft.EntityFrameworkCore;
using TradeManagementService.Data;
using System.Text.Json.Serialization;

namespace TradeManagementService;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // 📦 Добавляем сервисы
        builder.Services.AddControllers()
            .AddJsonOptions(options => {
                // 🔄 Настройка JSON для избежания циклических ссылок
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Сохраняем PascalCase
            });

        builder.Services.AddEndpointsApiExplorer();

        // 📚 Swagger
        builder.Services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new() {
                Title = "Trade Management Service",
                Version = "v1",
                Description = "API для управления трейдами и пользователями"
            });
        });

        // 🗄️ База данных (SQLite для простоты)
        builder.Services.AddDbContext<TradeDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // 🌐 CORS
        builder.Services.AddCors(options => {
            options.AddPolicy("AllowAll", policy => {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // ⚙️ Настройка pipeline
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trade Management Service v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapControllers();

        // 🗄️ Создаем базу данных при запуске
        using (var scope = app.Services.CreateScope()) {
            var context = scope.ServiceProvider.GetRequiredService<TradeDbContext>();
            context.Database.EnsureCreated();

            // Логируем статистику
            var tradesCount = context.Trades.Count();
            var usersCount = context.Users.Count();
            Console.WriteLine($"📊 Database initialized: {tradesCount} trades, {usersCount} users");
        }

        app.Run();
    }
}