using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeManagementService.Models;

public class Trade {
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    public decimal LoanAmount { get; set; }

    public int Years { get; set; }

    [MaxLength(50)]
    public string CalculationType { get; set; } = string.Empty; // "FixedRate", "FloatingRate", "OIS"

    [MaxLength(50)]
    public string? InterestType { get; set; } // "Simple", "Compound"

    public decimal TotalInterest { get; set; }

    public decimal TotalPayment { get; set; }

    // 📊 Дополнительные данные расчета (JSON)
    [Column(TypeName = "TEXT")]
    public string? CalculationData { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 🔗 Связь с пользователем
    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}