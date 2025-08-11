using DocumentTransformationService.Services;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace DocumentTransformationService;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // 📦 Добавляем сервисы
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // 📚 Swagger с поддержкой файлов
        builder.Services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new() {
                Title = "Document Transformation Service",
                Version = "v1",
                Description = "API для трансформации FpML/XML документов в торговые инструменты"
            });

            // 📁 Поддержка загрузки файлов в Swagger
            c.OperationFilter<FileUploadOperationFilter>();
        });

        // 🔧 HTTP клиенты для других сервисов
        builder.Services.AddHttpClient("ApiGateway", client => {
            client.BaseAddress = new Uri("https://localhost:7273/");
        });

        // 🧮 Регистрируем наши сервисы
        builder.Services.AddScoped<IFpMLParserService, FpMLParserService>();
        builder.Services.AddScoped<ITradeTransformationService, TradeTransformationService>();

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Transformation Service v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}

// 📁 Фильтр для поддержки загрузки файлов в Swagger

public class FileUploadOperationFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (fileParameters.Any()) {
            operation.RequestBody = new OpenApiRequestBody {
                Content = {
                    ["multipart/form-data"] = new OpenApiMediaType {
                        Schema = new OpenApiSchema {
                            Type = "object",
                            Properties = {
                                ["file"] = new OpenApiSchema {
                                    Type = "string",
                                    Format = "binary"
                                },
                                ["autoCalculate"] = new OpenApiSchema {
                                    Type = "boolean",
                                    Default = new Microsoft.OpenApi.Any.OpenApiBoolean(true)
                                },
                                ["notes"] = new OpenApiSchema {
                                    Type = "string"
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}