using Microsoft.EntityFrameworkCore;
using Nexus.Sales.API.Domain.Models;

namespace Nexus.Sales.API.Infra.Data;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(o => {
            o.HasKey(x => x.Id);
            o.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            o.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(i => {
            i.HasKey(x => x.Id);
            i.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}