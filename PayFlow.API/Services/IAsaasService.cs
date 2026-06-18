using PayFlow.Domain.Billing.Entities;

namespace PayFlow.API.Services;

public interface IAsaasService
{
    Task<AsaasSubaccountResult> CreateSubaccountAsync(
        AsaasSubaccountRequest request,
        CancellationToken cancellationToken = default);

    Task<string> CreateCustomerAsync(
        string apiKey,
        string name,
        string? cpfCnpj,
        string? mobilePhone,
        string? email,
        string? externalReference,
        CancellationToken cancellationToken = default);

    Task<string> CreatePlatformCustomerAsync(
        string name,
        string? cpfCnpj,
        string? mobilePhone,
        string? email,
        string? externalReference,
        CancellationToken cancellationToken = default);

    Task<AsaasChargeResult> CreatePaymentChargeAsync(
        Payment payment,
        string asaasCustomerId,
        string apiKey,
        string billingType,
        CancellationToken cancellationToken = default);

    Task<AsaasPaymentStatusResult> GetPaymentStatusAsync(
        string asaasPaymentId,
        string apiKey,
        CancellationToken cancellationToken = default);

    Task<AsaasSubscriptionResult> CreatePremiumSubscriptionAsync(
        string asaasCustomerId,
        Guid premiumSubscriptionId,
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(
        string asaasSubscriptionId,
        CancellationToken cancellationToken = default);

    bool IsConfigured { get; }
}

public record AsaasSubaccountRequest(
    string Name,
    string Email,
    string CpfCnpj,
    string MobilePhone,
    decimal IncomeValue,
    string Address,
    string AddressNumber,
    string Province,
    string PostalCode,
    string? CompanyType,
    string? Phone,
    string? Complement,
    string? Site);

public record AsaasSubaccountResult(
    string AccountId,
    string WalletId,
    string ApiKey);

public record AsaasChargeResult(
    string Id,
    string Status,
    string BillingType,
    string? InvoiceUrl,
    string? PixCopyPaste,
    string? PixQrCodeImageBase64,
    string? PixQrCodeSvg,
    DateTime? PixExpiresAt,
    decimal? FeeAmount,
    decimal? NetAmount);

public record AsaasPaymentStatusResult(
    string Id,
    string Status,
    string BillingType,
    string? InvoiceUrl,
    DateTime? PaymentDate,
    decimal? FeeAmount,
    decimal? NetAmount);

public record AsaasSubscriptionResult(
    string Id,
    string Status,
    string CustomerId,
    string? FirstPaymentUrl,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd);
