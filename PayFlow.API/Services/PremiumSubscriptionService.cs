using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayFlow.API.Auth;
using PayFlow.API.Data;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Billing.Entities;

namespace PayFlow.API.Services;

public interface IPremiumSubscriptionService
{
    Task<PremiumSubscriptionResponse> GetSubscriptionAsync(Guid userId, Guid sellerId, CancellationToken cancellationToken);
    Task<List<PremiumSubscriptionPaymentResponse>> GetPaymentsAsync(Guid userId, Guid sellerId, CancellationToken cancellationToken);
    Task<PremiumCheckoutResponse> CreatePremiumCheckoutAsync(Guid userId, Guid sellerId, string? email, CancellationToken cancellationToken);
    Task<PremiumSubscriptionResponse> CancelAsync(Guid userId, Guid sellerId, CancellationToken cancellationToken);
    Task<PremiumSubscriptionResponse> ReactivateAsync(Guid userId, Guid sellerId, CancellationToken cancellationToken);
    Task<WebhookProcessingResult> ProcessAsaasPaymentAsync(
        Guid subscriptionId,
        string asaasPaymentId,
        string status,
        decimal amount,
        string currency,
        string eventId,
        string eventType,
        DateTime? paidAt,
        CancellationToken cancellationToken);
}

public class PremiumSubscriptionService : IPremiumSubscriptionService
{
    private const string Provider = "asaas";

    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAsaasService _asaasService;
    private readonly INotificationService _notificationService;

    public PremiumSubscriptionService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IAsaasService asaasService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _asaasService = asaasService;
        _notificationService = notificationService;
    }

    public async Task<PremiumSubscriptionResponse> GetSubscriptionAsync(
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        var subscription = await GetSubscriptionByUserAsync(userId, cancellationToken);
        return subscription == null
            ? FreeSubscription(userId, sellerId)
            : Map(subscription);
    }

    public async Task<List<PremiumSubscriptionPaymentResponse>> GetPaymentsAsync(
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        EnsureValidIdentity(userId, sellerId);

        return await _dbContext.PremiumSubscriptionPayments
            .Where(payment => payment.UserId == userId && payment.SellerId == sellerId)
            .OrderByDescending(payment => payment.CreatedAt)
            .Take(100)
            .Select(payment => MapPayment(payment))
            .ToListAsync(cancellationToken);
    }

    public async Task<PremiumCheckoutResponse> CreatePremiumCheckoutAsync(
        Guid userId,
        Guid sellerId,
        string? email,
        CancellationToken cancellationToken)
    {
        EnsureAsaasConfigured();
        EnsureValidIdentity(userId, sellerId);

        var seller = await _dbContext.Sellers.FirstOrDefaultAsync(item => item.Id == sellerId, cancellationToken);
        if (seller == null)
            throw new NotFoundException("Vendedor não encontrado");

        var subscription = await GetOrCreateSubscriptionAsync(userId, sellerId, cancellationToken);
        if (subscription.IsActive && !subscription.CancelAtPeriodEnd)
            throw new ValidationException("Sua assinatura Premium já está ativa");

        var checkout = await CreateProviderSubscriptionAsync(subscription, seller.Name, email, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return checkout;
    }

    public async Task<PremiumSubscriptionResponse> CancelAsync(
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        EnsureAsaasConfigured();
        var subscription = await GetRequiredSubscriptionAsync(userId, sellerId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(subscription.ProviderSubscriptionId))
            await _asaasService.CancelSubscriptionAsync(subscription.ProviderSubscriptionId, cancellationToken);

        subscription.MarkCancelled();
        await _dbContext.SaveChangesAsync(cancellationToken);
        await SyncUserPremiumFlagAsync(subscription, cancellationToken);

        return Map(subscription);
    }

    public async Task<PremiumSubscriptionResponse> ReactivateAsync(
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        EnsureAsaasConfigured();
        var subscription = await GetRequiredSubscriptionAsync(userId, sellerId, cancellationToken);

        if (subscription.IsActive)
            throw new ValidationException("Sua assinatura Premium já está ativa");

        subscription.MarkReactivated();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(subscription);
    }

    public async Task<WebhookProcessingResult> ProcessAsaasPaymentAsync(
        Guid subscriptionId,
        string asaasPaymentId,
        string status,
        decimal amount,
        string currency,
        string eventId,
        string eventType,
        DateTime? paidAt,
        CancellationToken cancellationToken)
    {
        var subscription = await _dbContext.PremiumSubscriptions.FirstOrDefaultAsync(
            item => item.Id == subscriptionId,
            cancellationToken);

        if (subscription == null)
            return new WebhookProcessingResult(true, "Assinatura Premium não encontrada");

        var alreadyTracked = await _dbContext.PremiumSubscriptionPayments.AnyAsync(
            payment => payment.ExternalInvoiceId == asaasPaymentId && payment.Status == status,
            cancellationToken);

        if (!alreadyTracked)
        {
            _dbContext.PremiumSubscriptionPayments.Add(new PremiumSubscriptionPayment(
                subscription.Id,
                subscription.UserId,
                subscription.SellerId,
                Provider,
                status,
                amount,
                currency,
                externalInvoiceId: asaasPaymentId,
                externalProviderPaymentId: asaasPaymentId,
                paidAt: IsPaidStatus(status) ? paidAt : null,
                failureReason: IsFailureStatus(status) ? eventType : null));
        }

        var previousStatus = subscription.Status;
        if (IsPaidStatus(status))
            subscription.MarkActive(paidAt ?? DateTime.UtcNow, (paidAt ?? DateTime.UtcNow).AddMonths(1));
        else if (status.Equals("OVERDUE", StringComparison.OrdinalIgnoreCase) || IsFailureStatus(status))
            subscription.MarkPastDue();
        else if (status.Equals("CANCELED", StringComparison.OrdinalIgnoreCase) ||
                 status.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) ||
                 status.Equals("DELETED", StringComparison.OrdinalIgnoreCase))
            subscription.MarkCancelled();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await SyncUserPremiumFlagAsync(subscription, cancellationToken);
        NotifySubscriptionStatusChange(subscription, previousStatus, eventType);

        return new WebhookProcessingResult(true, "Pagamento Premium processado", null, subscription.SellerId);
    }

    private async Task<PremiumCheckoutResponse> CreateProviderSubscriptionAsync(
        PremiumSubscription subscription,
        string sellerName,
        string? email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subscription.ProviderCustomerId))
        {
            var customerId = await _asaasService.CreatePlatformCustomerAsync(
                string.IsNullOrWhiteSpace(sellerName) ? $"PayFlow {subscription.UserId}" : sellerName,
                null,
                null,
                email,
                $"premium-customer:{subscription.UserId}",
                cancellationToken);

            subscription.AttachProviderCustomer(customerId);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var result = await _asaasService.CreatePremiumSubscriptionAsync(
            subscription.ProviderCustomerId!,
            subscription.Id,
            subscription.UserId,
            subscription.SellerId,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(result.FirstPaymentUrl))
            throw new ValidationException("Asaas não retornou o link de pagamento da assinatura Premium");

        subscription.AttachProviderSubscription(
            result.Id,
            result.CustomerId,
            result.FirstPaymentUrl,
            result.CurrentPeriodStart,
            result.CurrentPeriodEnd);

        return new PremiumCheckoutResponse
        {
            SubscriptionId = result.Id,
            CheckoutUrl = result.FirstPaymentUrl
        };
    }

    private async Task<PremiumSubscription?> GetSubscriptionByUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.PremiumSubscriptions.FirstOrDefaultAsync(
            subscription => subscription.UserId == userId,
            cancellationToken);
    }

    private async Task<PremiumSubscription> GetRequiredSubscriptionAsync(
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        EnsureValidIdentity(userId, sellerId);

        var subscription = await _dbContext.PremiumSubscriptions.FirstOrDefaultAsync(
            item => item.UserId == userId && item.SellerId == sellerId,
            cancellationToken);

        if (subscription == null)
            throw new NotFoundException("Assinatura Premium não encontrada");

        return subscription;
    }

    private async Task<PremiumSubscription> GetOrCreateSubscriptionAsync(
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        EnsureValidIdentity(userId, sellerId);

        var subscription = await _dbContext.PremiumSubscriptions.FirstOrDefaultAsync(
            item => item.UserId == userId,
            cancellationToken);

        if (subscription != null)
            return subscription;

        subscription = new PremiumSubscription(userId, sellerId, Provider);
        _dbContext.PremiumSubscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    private async Task SyncUserPremiumFlagAsync(
        PremiumSubscription subscription,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(
            item => item.Id == subscription.UserId,
            cancellationToken);

        if (user == null || user.IsPremium == subscription.IsActive)
            return;

        user.IsPremium = subscription.IsActive;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ValidationException(string.Join("; ", result.Errors.Select(error => error.Description)));
    }

    private void NotifySubscriptionStatusChange(
        PremiumSubscription subscription,
        string previousStatus,
        string eventType)
    {
        if (subscription.Status == previousStatus)
            return;

        if (subscription.IsActive)
        {
            _notificationService.NotifySeller(
                subscription.SellerId,
                "premium_activated",
                "Assinatura Premium ativada",
                "Sua assinatura Premium está ativa.",
                subscription.UserId,
                "/profile",
                $"premium_activated:{subscription.Id}:{subscription.ProviderSubscriptionId}");
            return;
        }

        if (subscription.Status is "cancelled" or "expired")
        {
            _notificationService.NotifySeller(
                subscription.SellerId,
                "premium_expired",
                "Assinatura Premium expirada",
                "Sua assinatura Premium foi encerrada.",
                subscription.UserId,
                "/profile",
                $"premium_expired:{subscription.Id}:{eventType}");
        }
    }

    private static PremiumSubscriptionResponse FreeSubscription(Guid userId, Guid sellerId)
    {
        return new PremiumSubscriptionResponse
        {
            UserId = userId,
            SellerId = sellerId,
            Status = "free",
            StatusLabel = "Gratuito",
            IsPremium = false
        };
    }

    private static PremiumSubscriptionResponse Map(PremiumSubscription subscription)
    {
        return new PremiumSubscriptionResponse
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            SellerId = subscription.SellerId,
            Status = subscription.Status,
            StatusLabel = GetStatusLabel(subscription.Status, subscription.CancelAtPeriodEnd),
            IsPremium = subscription.IsActive,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            CancelledAt = subscription.CancelledAt,
            ProviderCustomerId = subscription.ProviderCustomerId,
            ProviderSubscriptionId = subscription.ProviderSubscriptionId,
            ProviderCheckoutUrl = subscription.ProviderCheckoutUrl
        };
    }

    private static PremiumSubscriptionPaymentResponse MapPayment(PremiumSubscriptionPayment payment)
    {
        return new PremiumSubscriptionPaymentResponse
        {
            Id = payment.Id,
            PremiumSubscriptionId = payment.PremiumSubscriptionId,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency,
            ExternalInvoiceId = payment.ExternalInvoiceId,
            ExternalProviderPaymentId = payment.ExternalProviderPaymentId,
            ExternalChargeId = payment.ExternalChargeId,
            PaidAt = payment.PaidAt,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt
        };
    }

    private static string GetStatusLabel(string status, bool cancelAtPeriodEnd)
    {
        if (cancelAtPeriodEnd && status == "active")
            return "Cancelamento agendado";

        return status switch
        {
            "active" => "Ativa",
            "past_due" => "Em atraso",
            "cancelled" => "Cancelada",
            "expired" => "Expirada",
            "pending" => "Pendente",
            _ => "Gratuito"
        };
    }

    private static bool IsPaidStatus(string status)
    {
        return status.Equals("RECEIVED", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("CONFIRMED", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("RECEIVED_IN_CASH", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("PAID", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFailureStatus(string status)
    {
        return status.Equals("REFUND_REQUESTED", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("CHARGEBACK_REQUESTED", StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureAsaasConfigured()
    {
        if (!_asaasService.IsConfigured)
            throw new ValidationException("Configure Asaas:ApiKey para usar assinatura Premium");
    }

    private static void EnsureValidIdentity(Guid userId, Guid sellerId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("Usuário não autenticado");

        if (sellerId == Guid.Empty)
            throw new UnauthorizedException("Vendedor não identificado");
    }
}
