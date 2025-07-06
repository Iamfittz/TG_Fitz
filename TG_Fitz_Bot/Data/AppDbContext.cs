using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fitz.Core.Models;

namespace TG_Fitz.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<InterestRates> InterestRates { get; set; }

        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        //optionsBuilder.UseSqlServer("Server=mytgdb.csxisym80s1w.us-east-1.rds.amazonaws.com;" +
        //        //    "Database=MyTGBotFitz;" +
        //        //    "User Id=admin;" +
        //        //    "Password=Armada732820777Qzwerty+;" +
        //        //    "TrustServerCertificate=True;",
        //        //    options => options.EnableRetryOnFailure());
        //        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        //        if (environment == "Development")
        //        {
        //            optionsBuilder.UseSqlite("Data Source=trade.db");
        //        }
        //        else
        //        {
        //            optionsBuilder.UseSqlite("Data Source=/home/ubuntu/TG_Fitz/bin/Release/net8.0/publish/trade.db");
        //        }
        //        //optionsBuilder.UseSqlite("Data Source=/home/ubuntu/TG_Fitz/bin/Release/net8.0/publish/trade.db");
        //    }
        //}
    }
}
