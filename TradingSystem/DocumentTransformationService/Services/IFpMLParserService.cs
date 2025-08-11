using DocumentTransformationService.Models.Trade;
using DocumentTransformationService.Models.Gateway;
using DocumentTransformationService.Models;

namespace DocumentTransformationService.Services;

public interface IFpMLParserService {

    // Парсит FpML XML документ в структурированную модель
    
    Task<ParsedTrade> ParseFpMLAsync(string xmlContent);

    /// <summary>
    /// ✅ Проверяет валидность FpML документа
    /// </summary>
    bool IsValidFpML(string xmlContent);

    /// <summary>
    /// 🔍 Определяет тип инструмента из FpML
    /// </summary>
    string GetInstrumentType(string xmlContent);
}

public interface ITradeTransformationService {
    /// <summary>
    /// 🔄 Трансформирует ParsedTrade в запрос для ApiGateway
    /// </summary>
    Task<ApiGatewayTradeRequest> TransformToApiGatewayRequestAsync(ParsedTrade parsedTrade, long? telegramUserId = null);

    /// <summary>
    /// 🧮 Отправляет трансформированную сделку в ApiGateway для расчета
    /// </summary>
    Task<object> SendToCalculationAsync(ApiGatewayTradeRequest request);
}