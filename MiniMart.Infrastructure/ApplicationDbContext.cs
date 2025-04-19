using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using MiniMart.Domain.Models;

namespace MiniMart.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions opt): base(opt)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductInventory>()
                        .ToTable(t => t.HasCheckConstraint("CK_ProductInventory_Quantity", "Quantity >= 0")); // last line of defense for race condition effect
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<TransactionQueryLog> TransactionQueryLogs { get; set; }
    }
}
