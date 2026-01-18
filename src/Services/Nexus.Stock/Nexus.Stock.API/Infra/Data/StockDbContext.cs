using Microsoft.EntityFrameworkCore;
using Nexus.Stock.API.Domain.Models;

namespace Nexus.Stock.API.Infra.Data;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração do Produto
        modelBuilder.Entity<Product>(p => {
            p.HasKey(x => x.Id);
            p.Property(x => x.Name).IsRequired().HasMaxLength(100);
            p.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
            p.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId);
        });

        // Configuração da Variação
        modelBuilder.Entity<ProductVariant>(v => {
            v.HasKey(x => x.Id);
            v.Property(x => x.Size).HasMaxLength(20);
            v.Property(x => x.Color).HasMaxLength(30);
            
            // O SEGREDO DA CONCORRÊNCIA (Timestamp no SQL Server)
            v.Property(x => x.RowVersion).IsRowVersion();
        });
    }
}