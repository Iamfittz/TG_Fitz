using Microsoft.EntityFrameworkCore;
using TradeManagementService.Models;

namespace TradeManagementService.Data;

public class TradeDbContext : DbContext {
    public TradeDbContext(DbContextOptions<TradeDbContext> options) : base(options) {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // 🔗 Настройка связей
        modelBuilder.Entity<Trade>()
            .HasOne(t => t.User)
            .WithMany(u => u.Trades)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 📊 Индексы для быстрого поиска
        modelBuilder.Entity<User>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

        modelBuilder.Entity<Trade>()
            .HasIndex(t => t.CreatedAt);

        modelBuilder.Entity<Trade>()
            .HasIndex(t => t.CalculationType);
    }
}