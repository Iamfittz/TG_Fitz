using Microsoft.EntityFrameworkCore;
using TradeManagementService.Data;

namespace TradeManagementService;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // 📦 Добавляем сервисы
        builder.Services.AddControllers();
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
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapControllers();

        // 🗄️ Создаем базу данных при запуске
        using (var scope = app.Services.CreateScope()) {
            var context = scope.ServiceProvider.GetRequiredService<TradeDbContext>();
            context.Database.EnsureCreated();
        }

        app.Run();
    }
}