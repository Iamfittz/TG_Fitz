using System.ComponentModel.DataAnnotations;

namespace TradeManagementService.Models;

public class User {
    [Key]
    public int Id { get; set; }

    [Required]
    public long TelegramId { get; set; }

    [MaxLength(255)]
    public string? Username { get; set; }

    [MaxLength(255)]
    public string? FirstName { get; set; }

    [MaxLength(255)]
    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 🔗 Связь с трейдами
    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}