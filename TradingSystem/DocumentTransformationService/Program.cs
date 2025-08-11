using DocumentTransformationService.Services;
using DocumentTransformationService.Models;


namespace DocumentTransformationService;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // 📦 Добавляем сервисы
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // 📚 Swagger
        builder.Services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new() {
                Title = "Document Transformation Service",
                Version = "v1",
                Description = "API для трансформации FpML/XML документов в торговые инструменты"
            });
        });

        // 🔧 HTTP клиенты для других сервисов
        builder.Services.AddHttpClient("ApiGateway", client => {
            client.BaseAddress = new Uri("https://localhost:7273/");
        });

        // 🧮 Регистрируем наши сервисы
        // 🧮 Регистрируем наши сервисы ПРАВИЛЬНО
        //builder.Services.AddScoped<DocumentTransformationService.Services.IFpMLParserService, DocumentTransformationService.Services.FpMLParserService>();
        //builder.Services.AddScoped<DocumentTransformationService.Services.ITradeTransformationService, DocumentTransformationService.Services.TradeTransformationService>();

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

        app.Run();
    }
}