namespace ApiGateway;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // 📦 Добавляем сервисы
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // 📚 Swagger для Gateway
        builder.Services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new() {
                Title = "Trading System API Gateway",
                Version = "v1",
                Description = "Единая точка входа для всех сервисов Trading System"
            });
        });

        // 🌉 YARP Reverse Proxy
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        // 🌐 CORS
        builder.Services.AddCors(options => {
            options.AddPolicy("AllowAll", policy => {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // 🔧 HTTP клиенты для прямых вызовов
        builder.Services.AddHttpClient("CalculationService", client => {
            client.BaseAddress = new Uri("https://localhost:7299/");
        });

        builder.Services.AddHttpClient("TradeService", client => {
            client.BaseAddress = new Uri("https://localhost:7020/");
        });

        var app = builder.Build();

        // ⚙️ Настройка pipeline
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
            c.RoutePrefix = "swagger"; // Swagger на /swagger
        });

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseRouting();

        // 🎛️ Сначала наши контроллеры, потом прокси
        app.MapControllers();

        // 🌉 Затем YARP прокси для остального
        app.MapReverseProxy();

        app.Run();
    }
}