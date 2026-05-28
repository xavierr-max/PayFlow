using Microsoft.EntityFrameworkCore;
using PayFlow.Domain.Sales.Entities;
using PayFlow.Domain.Billing.Entities;

namespace PayFlow.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Seller> Sellers { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<PaymentItem> PaymentItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Seller>().HasKey(e => e.Id);
        modelBuilder.Entity<Category>().HasKey(e => e.Id);
        modelBuilder.Entity<Product>().HasKey(e => e.Id);
        modelBuilder.Entity<Customer>().HasKey(e => e.Id);
        modelBuilder.Entity<Payment>().HasKey(e => e.Id);
        modelBuilder.Entity<PaymentItem>().HasKey(e => new { e.PaymentId, e.ProductId });

        // Configure relationships and property settings if needed
    }
}
