namespace DocumentTransformationService.Models.Gateway;

/// <summary>
/// 🎯 Модель для отправки трансформированной сделки в ApiGateway
/// </summary>
public class ApiGatewayTradeRequest {
    // User information
    public long TelegramId { get; set; }
    public string? Username { get; set; } = "FpML_User";
    public string? FirstName { get; set; } = "Document";
    public string? LastName { get; set; } = "Import";

    // Trade basic info
    public string CompanyName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public int Years { get; set; }
    public string CalculationType { get; set; } = "FixedRate"; // "FixedRate", "FloatingRate", "OIS"
    public string? InterestType { get; set; } = "Simple";

    // FixedRate specific
    public decimal[]? YearlyRates { get; set; }

    // FloatingRate specific
    public decimal[]? FloatingRates { get; set; }
    public int ResetPeriod { get; set; }

    // OIS specific
    public decimal OvernightRate { get; set; }
    public int Days { get; set; }
    public int DayCountConvention { get; set; }

    // Metadata
    public string? SourceDocument { get; set; } = "FpML";
    public string? TransformationId { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}