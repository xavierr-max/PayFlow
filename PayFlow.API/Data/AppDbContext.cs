using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayFlow.API.Auth;
using PayFlow.Domain.Sales.Entities;
using PayFlow.Domain.Billing.Entities;
using PayFlow.Domain.Notifications.Entities;

namespace PayFlow.API.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
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
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
    public DbSet<WebhookEventLog> WebhookEventLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<PremiumSubscription> PremiumSubscriptions { get; set; } = null!;
    public DbSet<PremiumSubscriptionPayment> PremiumSubscriptionPayments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Seller>().HasKey(e => e.Id);
        modelBuilder.Entity<Category>().HasKey(e => e.Id);
        modelBuilder.Entity<Product>().HasKey(e => e.Id);
        modelBuilder.Entity<Customer>().HasKey(e => e.Id);
        modelBuilder.Entity<Payment>().HasKey(e => e.Id);
        modelBuilder.Entity<PaymentItem>().HasKey(e => new { e.PaymentId, e.ProductId });
        modelBuilder.Entity<PaymentTransaction>().HasKey(e => e.Id);
        modelBuilder.Entity<WebhookEventLog>().HasKey(e => e.Id);
        modelBuilder.Entity<Notification>().HasKey(e => e.Id);
        modelBuilder.Entity<PremiumSubscription>().HasKey(e => e.Id);
        modelBuilder.Entity<PremiumSubscriptionPayment>().HasKey(e => e.Id);

        modelBuilder.Entity<Payment>()
            .HasIndex(e => e.SellerId);

        modelBuilder.Entity<Payment>()
            .HasIndex(e => e.AsaasPaymentId);

        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(e => e.PaymentId);

        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(e => e.SellerId);

        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(e => e.ExternalProviderPaymentId);

        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(e => e.ExternalChargeId);

        modelBuilder.Entity<WebhookEventLog>()
            .HasIndex(e => new { e.Provider, e.EventId })
            .IsUnique();

        modelBuilder.Entity<Notification>()
            .HasIndex(e => e.SellerId);

        modelBuilder.Entity<Notification>()
            .HasIndex(e => new { e.SellerId, e.IsRead });

        modelBuilder.Entity<Notification>()
            .HasIndex(e => e.DeduplicationKey);

        modelBuilder.Entity<PremiumSubscription>()
            .HasIndex(e => e.UserId)
            .IsUnique();

        modelBuilder.Entity<PremiumSubscription>()
            .HasIndex(e => e.SellerId);

        modelBuilder.Entity<PremiumSubscription>()
            .HasIndex(e => e.ProviderSubscriptionId);

        modelBuilder.Entity<PremiumSubscription>()
            .HasIndex(e => e.ProviderCustomerId);

        modelBuilder.Entity<PremiumSubscriptionPayment>()
            .HasIndex(e => e.PremiumSubscriptionId);

        modelBuilder.Entity<PremiumSubscriptionPayment>()
            .HasIndex(e => e.ExternalInvoiceId);

        // Configure relationships and property settings if needed
    }
}
