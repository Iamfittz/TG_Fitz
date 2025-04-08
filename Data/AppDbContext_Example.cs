//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;

//namespace TG_Fitz.Data
//{
//    public class AppDbContext : DbContext
//    {
//        public DbSet<User> Users { get; set; }
//        public DbSet<Trade> Trades { get; set; }
//        public DbSet<InterestRates> InterestRates { get; set; }

//        public AppDbContext() { }
//        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            if (!optionsBuilder.IsConfigured)
//            {
//                optionsBuilder.UseSqlite("Data Source = trade.db");
//            }
//        }
//    }
//}
