using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CalculationService.Core.Enums; 

namespace TradeManagementService.Models;

public class Trade {
    [Key]
    public int Id { get; set; } // Оставляем для EF, но добавляем оригинальный ID

    // NEW: Original FpML data
    [Required]
    [MaxLength(255)]
    public string OriginalTradeId { get; set; } = string.Empty; 

    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    public decimal LoanAmount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD"; // SEK, USD, EUR

    public int Years { get; set; }

    // NEW: FpML dates
    public DateTime? EffectiveDate { get; set; } // 2018-11-06
    public DateTime? TerminationDate { get; set; } // 2023-11-06
    public DateTime? TradeDate { get; set; } // 2018-11-06

    // Using existing enums!
    public CalculationType CalculationType { get; set; } = CalculationType.None;
    public InterestCalculationType? InterestType { get; set; } // Simple, Compound

    // NEW: Fixed leg details
    public decimal? FixedRate { get; set; } 
    public DayCountConvention? FixedLegDayCount { get; set; } 
    [MaxLength(20)]
    public string? FixedLegPaymentFreq { get; set; } // "1Y"

    // NEW: Floating leg details
    [MaxLength(50)]
    public string? FloatingRateIndex { get; set; } // "SEK-STIBOR-SIDE"
    [MaxLength(20)]
    public string? FloatingLegTenor { get; set; } // "3M"
    public DayCountConvention? FloatingLegDayCount { get; set; } 
    [MaxLength(20)]
    public string? FloatingLegPaymentFreq { get; set; } // "3M"

    // NEW: Business day conventions
    [MaxLength(50)]
    public BusinessDayConvention? BusinessDayConvention { get; set; } // "MODFOLLOWING"
    [MaxLength(100)]
    public string? BusinessCenters { get; set; } // "SEST"

    // NEW: Parties
    [MaxLength(255)]
    public string? PayerParty { get; set; } // partyA/partyB
    [MaxLength(255)]
    public string? ReceiverParty { get; set; } // partyA/partyB
    [MaxLength(255)]
    public string? CounterpartyName { get; set; } // "SELL SECURITIES CO LTD"

    // Calculation results
    public decimal TotalInterest { get; set; }
    public decimal TotalPayment { get; set; }

    // Additional calculation data (JSON)
    [Column(TypeName = "TEXT")]
    public string? CalculationData { get; set; }

    // NEW: Original XML content
    [Column(TypeName = "TEXT")]
    public string? OriginalXmlContent { get; set; } // Store entire XML!

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 🔗 Relation with user
    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}