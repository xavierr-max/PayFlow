using System.Globalization;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Billing.Entities;
using PayFlow.Domain.Sales.Entities;
using PayFlow.Domain.Sales.Enum;

namespace PayFlow.API.Services;

public interface IPaymentService
{
    PaymentResponse CreatePayment(Guid sellerId, CreatePaymentRequest request);
    PaymentResponse GetPayment(Guid id);
    List<PaymentResponse> GetPaymentsBySeller(Guid sellerId);
    List<PaymentResponse> GetPaymentsByStatus(Guid sellerId, string status);
    PaymentResponse MarkAsPaid(Guid id, MarkPaymentPaidRequest request, Guid? actorUserId, string? actorEmail);
    PaymentResponse UnmarkAsPaid(Guid id, UnmarkPaymentPaidRequest request, Guid? actorUserId, string? actorEmail);
    PaymentResponse CancelPayment(Guid id, CancelPaymentRequest request, Guid? actorUserId, string? actorEmail);
    PaymentResponse UpdatePaymentDates(Guid id, UpdatePaymentDatesRequest request, Guid? actorUserId, string? actorEmail);
    Task<PaymentResponse> CreatePixWhatsappMessage(Guid id, CancellationToken cancellationToken);
    Task<PaymentResponse> CreateWhatsappPaymentMessage(Guid id, CreatePaymentWhatsappMessageRequest request, CancellationToken cancellationToken);
    Task<PaymentResponse> SyncPaymentStatus(Guid id, CancellationToken cancellationToken);
    Task<PaymentResponse> RefundPayment(Guid id, RefundPaymentRequest request, CancellationToken cancellationToken);
    List<PaymentTransactionResponse> GetPaymentTransactions(Guid id);
    void DeletePayment(Guid id);
}

public class PaymentService : IPaymentService
{
    private readonly IDataRepository _repository;
    private readonly IAsaasService _asaasService;
    private readonly INotificationService _notificationService;

    public PaymentService(
        IDataRepository repository,
        IAsaasService asaasService,
        INotificationService notificationService)
    {
        _repository = repository;
        _asaasService = asaasService;
        _notificationService = notificationService;
    }

    public PaymentResponse CreatePayment(Guid sellerId, CreatePaymentRequest request)
    {
        var customer = _repository.GetCustomer(request.CustomerId);
        if (customer == null || customer.SellerId != sellerId)
            throw new NotFoundException($"Customer with ID {request.CustomerId} not found for this seller");

        foreach (var item in request.Items)
        {
            var product = _repository.GetProduct(item.ProductId);
            if (product == null || product.SellerId != sellerId)
                throw new ValidationException($"Product with ID {item.ProductId} does not belong to this seller");
        }

        var payment = new Payment(
            request.DueDate,
            request.Amount,
            request.InstallmentNumber,
            request.TotalInstallments,
            request.CustomerId,
            sellerId);

        _repository.AddPayment(payment);
        NotifyPaymentCreated(payment, customer.Name);

        if (request.Items.Count > 0)
        {
            var items = request.Items
                .Select(item => new PaymentItem(item.ProductId, payment.Id, item.Quantity, item.UnitPrice))
                .ToList();

            _repository.AddPaymentItems(items);
        }

        return MapToResponse(payment, customer.Name, customer.Phone, customer.SellerId);
    }

    public PaymentResponse GetPayment(Guid id)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "", customer?.Phone ?? "", payment.SellerId);
    }

    public List<PaymentResponse> GetPaymentsBySeller(Guid sellerId)
    {
        var payments = _repository.GetPaymentsBySeller(sellerId);
        MarkOverduePayments(payments);
        return payments.Select(p =>
        {
            var customer = _repository.GetCustomer(p.CustomerId);
            return MapToResponse(p, customer?.Name ?? "", customer?.Phone ?? "", p.SellerId);
        }).ToList();
    }

    public List<PaymentResponse> GetPaymentsByStatus(Guid sellerId, string status)
    {
        if (!Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            throw new ValidationException($"Invalid payment status: {status}");

        var payments = _repository.GetPaymentsByStatus(sellerId, paymentStatus);
        MarkOverduePayments(payments);
        return payments.Select(p =>
        {
            var customer = _repository.GetCustomer(p.CustomerId);
            return MapToResponse(p, customer?.Name ?? "", customer?.Phone ?? "", p.SellerId);
        }).ToList();
    }

    public PaymentResponse MarkAsPaid(Guid id, MarkPaymentPaidRequest request, Guid? actorUserId, string? actorEmail)
    {
        var payment = GetPaymentEntity(id);

        payment.MarkAsPaidManually(request.ValueReceived, actorUserId, request.Reason, request.PaidAt);
        _repository.UpdatePayment(payment);
        _repository.AddPaymentTransaction(new PaymentTransaction(
            payment.Id,
            payment.SellerId,
            "manual",
            "manual_confirmation",
            "paid",
            request.ValueReceived,
            "brl",
            actorUserId: actorUserId,
            actorEmail: actorEmail,
            reason: request.Reason,
            notes: "Pagamento confirmado manualmente"));
        _notificationService.NotifySeller(
            payment.SellerId,
            "payment_manual_paid",
            "Cobrança marcada como paga",
            $"A cobrança {payment.TxId} foi confirmada manualmente.",
            actorUserId,
            $"/payments/{payment.Id}",
            $"payment_manual_paid:{payment.Id}:{payment.ManualPaidAt:O}");

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "", customer?.Phone ?? "", payment.SellerId);
    }

    public PaymentResponse UnmarkAsPaid(Guid id, UnmarkPaymentPaidRequest request, Guid? actorUserId, string? actorEmail)
    {
        var payment = GetPaymentEntity(id);

        var nextStatus = payment.UndoManualPayment(actorUserId, request.Reason);
        _repository.UpdatePayment(payment);
        _repository.AddPaymentTransaction(new PaymentTransaction(
            payment.Id,
            payment.SellerId,
            "manual",
            "manual_confirmation_reversed",
            nextStatus.ToString(),
            payment.Amount,
            "brl",
            actorUserId: actorUserId,
            actorEmail: actorEmail,
            reason: request.Reason,
            notes: "Confirmação manual de pagamento desfeita"));
        _notificationService.NotifySeller(
            payment.SellerId,
            "payment_manual_unpaid",
            "Pagamento manual desfeito",
            $"A cobrança {payment.TxId} voltou para {GetStatusLabel(payment.Status).ToLowerInvariant()}.",
            actorUserId,
            $"/payments/{payment.Id}",
            $"payment_manual_unpaid:{payment.Id}:{payment.ManualPaymentReversedAt:O}");

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "", customer?.Phone ?? "", payment.SellerId);
    }

    public PaymentResponse CancelPayment(Guid id, CancelPaymentRequest request, Guid? actorUserId, string? actorEmail)
    {
        var payment = GetPaymentEntity(id);

        payment.MarkAsCancelled();
        _repository.UpdatePayment(payment);
        _repository.AddPaymentTransaction(new PaymentTransaction(
            payment.Id,
            payment.SellerId,
            "manual",
            "payment_cancelled",
            "cancelled",
            payment.Amount,
            "brl",
            actorUserId: actorUserId,
            actorEmail: actorEmail,
            reason: request.Reason,
            notes: "Cobrança cancelada manualmente"));
        _notificationService.NotifySeller(
            payment.SellerId,
            "payment_cancelled",
            "Cobrança cancelada",
            $"A cobrança {payment.TxId} foi cancelada.",
            actorUserId,
            $"/payments/{payment.Id}",
            $"payment_cancelled:{payment.Id}");

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "", customer?.Phone ?? "", payment.SellerId);
    }

    public PaymentResponse UpdatePaymentDates(Guid id, UpdatePaymentDatesRequest request, Guid? actorUserId, string? actorEmail)
    {
        var payment = GetPaymentEntity(id);

        if (request.DueDate.HasValue)
            payment.UpdateDueDate(request.DueDate.Value);

        if (request.PaidAt.HasValue && payment.Status == PaymentStatus.Paid)
            payment.MarkAsPaid(payment.Amount, request.PaidAt);

        _repository.UpdatePayment(payment);
        _repository.AddPaymentTransaction(new PaymentTransaction(
            payment.Id,
            payment.SellerId,
            "manual",
            "dates_updated",
            payment.Status.ToString(),
            payment.Amount,
            "brl",
            actorUserId: actorUserId,
            actorEmail: actorEmail,
            notes: "Datas da cobrança atualizadas"));

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "", customer?.Phone ?? "", payment.SellerId);
    }

    public async Task<PaymentResponse> CreatePixWhatsappMessage(Guid id, CancellationToken cancellationToken)
    {
        return await CreateWhatsappPaymentMessage(
            id,
            new CreatePaymentWhatsappMessageRequest { PaymentMethod = "pix" },
            cancellationToken);
    }

    public async Task<PaymentResponse> CreateWhatsappPaymentMessage(
        Guid id,
        CreatePaymentWhatsappMessageRequest request,
        CancellationToken cancellationToken)
    {
        var payment = GetPaymentEntity(id);

        if (payment.Status == PaymentStatus.Paid)
            throw new ValidationException("Não é possível gerar link para uma cobrança já paga");

        var customer = _repository.GetCustomer(payment.CustomerId);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {payment.CustomerId} not found");

        var seller = _repository.GetSeller(payment.SellerId);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {payment.SellerId} not found");

        EnsureSellerAsaasReady(seller);

        var paymentMethod = NormalizePaymentMethod(request.PaymentMethod);
        var billingType = ToAsaasBillingType(paymentMethod);
        var asaasCustomerId = await EnsureAsaasCustomerAsync(customer, seller, cancellationToken);

        if (paymentMethod == "pix")
        {
            if (!payment.HasActivePix(DateTime.UtcNow))
                await CreateAsaasChargeAsync(payment, seller, asaasCustomerId, billingType, paymentMethod, cancellationToken);

            payment.MarkPixMessageSent();
        }
        else
        {
            if (!payment.HasActivePaymentLink(billingType))
                await CreateAsaasChargeAsync(payment, seller, asaasCustomerId, billingType, paymentMethod, cancellationToken);

            payment.MarkPaymentMessageSent();
        }

        _repository.UpdatePayment(payment);
        return MapToResponse(payment, customer.Name, customer.Phone, payment.SellerId);
    }

    public async Task<PaymentResponse> SyncPaymentStatus(Guid id, CancellationToken cancellationToken)
    {
        var payment = GetPaymentEntity(id);
        if (string.IsNullOrWhiteSpace(payment.AsaasPaymentId))
            throw new ValidationException("Esta cobrança ainda não possui pagamento Asaas para sincronizar");

        var seller = _repository.GetSeller(payment.SellerId);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {payment.SellerId} not found");

        EnsureSellerAsaasReady(seller);

        var status = await _asaasService.GetPaymentStatusAsync(payment.AsaasPaymentId, seller.AsaasApiKey!, cancellationToken);
        payment.SyncAsaasPaymentStatus(status.Status, status.PaymentDate);
        payment.AttachAsaasFinancialDetails(status.FeeAmount, status.NetAmount);
        _repository.UpdatePayment(payment);
        _repository.AddPaymentTransaction(new PaymentTransaction(
            payment.Id,
            payment.SellerId,
            "asaas",
            "status_sync",
            status.Status,
            payment.Amount,
            "brl",
            paymentMethod: payment.AsaasBillingType,
            providerAccountId: seller.AsaasAccountId,
            externalProviderPaymentId: status.Id,
            notes: "Status sincronizado manualmente com o Asaas"));

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "", customer?.Phone ?? "", payment.SellerId);
    }

    public Task<PaymentResponse> RefundPayment(Guid id, RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        _ = GetPaymentEntity(id);
        throw new ValidationException("Estorno via Asaas ainda não está implementado nesta API.");
    }

    public List<PaymentTransactionResponse> GetPaymentTransactions(Guid id)
    {
        _ = GetPaymentEntity(id);
        return _repository.GetPaymentTransactions(id).Select(MapTransactionToResponse).ToList();
    }

    public void DeletePayment(Guid id)
    {
        _ = GetPaymentEntity(id);
        _repository.DeletePayment(id);
    }

    private async Task CreateAsaasChargeAsync(
        Payment payment,
        Seller seller,
        string asaasCustomerId,
        string billingType,
        string paymentMethod,
        CancellationToken cancellationToken)
    {
        var charge = await _asaasService.CreatePaymentChargeAsync(
            payment,
            asaasCustomerId,
            seller.AsaasApiKey!,
            billingType,
            cancellationToken);

        payment.AttachAsaasPayment(
            charge.Id,
            charge.Status,
            charge.BillingType,
            asaasCustomerId,
            charge.InvoiceUrl,
            charge.PixCopyPaste,
            charge.PixQrCodeImageBase64,
            charge.PixQrCodeSvg,
            charge.PixExpiresAt);
        payment.AttachAsaasFinancialDetails(charge.FeeAmount, charge.NetAmount);

        _repository.AddPaymentTransaction(new PaymentTransaction(
            payment.Id,
            payment.SellerId,
            "asaas",
            paymentMethod == "pix" ? "pix_created" : "card_payment_link_created",
            charge.Status,
            payment.Amount,
            "brl",
            paymentMethod: paymentMethod,
            providerAccountId: seller.AsaasAccountId,
            externalProviderPaymentId: charge.Id,
            notes: paymentMethod == "pix"
                ? "Cobrança PIX dinâmica criada no Asaas"
                : "Cobrança de cartão criada no Asaas"));
    }

    private async Task<string> EnsureAsaasCustomerAsync(
        Customer customer,
        Seller seller,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(customer.AsaasCustomerId))
            return customer.AsaasCustomerId;

        var asaasCustomerId = await _asaasService.CreateCustomerAsync(
            seller.AsaasApiKey!,
            customer.Name,
            customer.CpfCnpj,
            customer.Phone,
            customer.Email,
            customer.Id.ToString(),
            cancellationToken);

        customer.SetAsaasCustomerId(asaasCustomerId);
        _repository.UpdateCustomer(customer);
        return asaasCustomerId;
    }

    private static void EnsureSellerAsaasReady(Seller seller)
    {
        if (seller.HasAsaasSubaccount())
            return;

        var missing = seller.GetMissingAsaasSubaccountFields();
        var suffix = missing.Count == 0
            ? "crie a subconta Asaas do vendedor antes de gerar cobranças"
            : "complete os dados do vendedor para criar a subconta Asaas: " + string.Join(", ", missing);

        throw new ValidationException("O vendedor precisa de uma subconta Asaas ativa; " + suffix);
    }

    private Payment GetPaymentEntity(Guid id)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        return payment;
    }

    private static string GetStatusLabel(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "Pendente",
            PaymentStatus.Paid => "Pago",
            PaymentStatus.Overdue => "Atrasado",
            PaymentStatus.Cancelled => "Cancelado",
            PaymentStatus.Failed => "Recusado",
            PaymentStatus.Expired => "Expirado",
            PaymentStatus.Refunded => "Estornado",
            PaymentStatus.PartiallyRefunded => "Parcialmente estornado",
            PaymentStatus.Chargeback => "Chargeback",
            _ => "Desconhecido"
        };
    }

    private static PaymentResponse MapToResponse(Payment payment, string customerName, string customerPhone, Guid sellerId)
    {
        var whatsappMessage = BuildWhatsappMessage(payment, customerName);
        var whatsappUrl = BuildWhatsappUrl(customerPhone, whatsappMessage);

        return new PaymentResponse
        {
            Id = payment.Id,
            Status = (int)payment.Status,
            StatusLabel = GetStatusLabel(payment.Status),
            Amount = payment.Amount,
            InstallmentNumber = payment.InstallmentNumber,
            TotalInstallments = payment.TotalInstallments,
            DueDate = payment.DueDate,
            PaidAt = payment.PaidAt,
            TxId = payment.TxId,
            CustomerId = payment.CustomerId,
            SellerId = sellerId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            AsaasPaymentId = payment.AsaasPaymentId,
            AsaasPaymentStatus = payment.AsaasPaymentStatus,
            AsaasBillingType = payment.AsaasBillingType,
            AsaasInvoiceUrl = payment.AsaasInvoiceUrl,
            AsaasCustomerId = payment.AsaasCustomerId,
            PixCopyPaste = payment.PixCopyPaste,
            PixQrCodeImageUrl = payment.PixQrCodeImageUrl,
            PixQrCodeSvgUrl = payment.PixQrCodeSvgUrl,
            PixHostedInstructionsUrl = payment.PixHostedInstructionsUrl,
            PixExpiresAt = payment.PixExpiresAt,
            PixMessageSentAt = payment.PixMessageSentAt,
            PaymentMessageSentAt = payment.PaymentMessageSentAt,
            RefundedAmount = payment.RefundedAmount,
            FeeAmount = payment.FeeAmount,
            NetAmount = payment.NetAmount,
            CardInstallmentCount = payment.CardInstallmentCount,
            CardBrand = payment.CardBrand,
            CardLast4 = payment.CardLast4,
            ManualPaidAt = payment.ManualPaidAt,
            ManualPaidByUserId = payment.ManualPaidByUserId,
            ManualPaidReason = payment.ManualPaidReason,
            ManualPaymentReversedAt = payment.ManualPaymentReversedAt,
            ManualPaymentReversedByUserId = payment.ManualPaymentReversedByUserId,
            ManualPaymentReversalReason = payment.ManualPaymentReversalReason,
            WhatsappMessage = whatsappMessage,
            WhatsappUrl = whatsappUrl
        };
    }

    private static PaymentTransactionResponse MapTransactionToResponse(PaymentTransaction transaction)
    {
        return new PaymentTransactionResponse
        {
            Id = transaction.Id,
            PaymentId = transaction.PaymentId,
            SellerId = transaction.SellerId,
            Provider = transaction.Provider,
            Type = transaction.Type,
            Status = transaction.Status,
            Amount = transaction.Amount,
            FeeAmount = transaction.FeeAmount,
            NetAmount = transaction.NetAmount,
            Currency = transaction.Currency,
            PaymentMethod = transaction.PaymentMethod,
            InstallmentCount = transaction.InstallmentCount,
            ProviderAccountId = transaction.ProviderAccountId,
            ProviderEventId = transaction.ProviderEventId,
            ExternalTransactionId = transaction.ExternalTransactionId,
            ExternalProviderPaymentId = transaction.ExternalProviderPaymentId,
            ExternalChargeId = transaction.ExternalChargeId,
            ExternalRefundId = transaction.ExternalRefundId,
            ActorUserId = transaction.ActorUserId,
            ActorEmail = transaction.ActorEmail,
            Reason = transaction.Reason,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt,
            OccurredAt = transaction.OccurredAt
        };
    }

    private void MarkOverduePayments(List<Payment> payments)
    {
        foreach (var payment in payments)
        {
            if (payment.Status != PaymentStatus.Pending || DateTime.UtcNow <= payment.DueDate)
                continue;

            payment.MarkAsOverdue();
            _repository.UpdatePayment(payment);
            _repository.AddPaymentTransaction(new PaymentTransaction(
                payment.Id,
                payment.SellerId,
                "system",
                "payment_overdue",
                "overdue",
                payment.Amount,
                "brl",
                notes: "Cobrança vencida pelo relógio do sistema"));
            _notificationService.NotifySeller(
                payment.SellerId,
                "payment_overdue",
                "Cobrança vencida",
                $"A cobrança {payment.TxId} venceu em {payment.DueDate:dd/MM/yyyy}.",
                targetUrl: $"/payments/{payment.Id}",
                deduplicationKey: $"payment_overdue:{payment.Id}");
        }
    }

    private void NotifyPaymentCreated(Payment payment, string customerName)
    {
        _notificationService.NotifySeller(
            payment.SellerId,
            "payment_created",
            "Nova cobrança criada",
            $"Cobrança de {payment.Amount.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))} para {customerName}.",
            targetUrl: $"/payments/{payment.Id}",
            deduplicationKey: $"payment_created:{payment.Id}");
    }

    private static string? BuildWhatsappMessage(Payment payment, string customerName)
    {
        if (payment.AsaasBillingType?.Equals("CREDIT_CARD", StringComparison.OrdinalIgnoreCase) == true)
            return BuildCardWhatsappMessage(payment, customerName);

        if (string.IsNullOrWhiteSpace(payment.PixCopyPaste) && string.IsNullOrWhiteSpace(payment.AsaasInvoiceUrl))
            return null;

        var amount = payment.Amount.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        var dueDate = payment.DueDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));
        var expiresAt = payment.PixExpiresAt?.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR"));

        var message = $"Olá, {customerName}. Sua cobrança PayFlow de {amount} vence em {dueDate}.";

        if (!string.IsNullOrWhiteSpace(payment.AsaasInvoiceUrl))
            message += $"\n\nLink de pagamento:\n{payment.AsaasInvoiceUrl}";

        if (!string.IsNullOrWhiteSpace(payment.PixCopyPaste))
            message += $"\n\nPix copia e cola:\n{payment.PixCopyPaste}";

        if (!string.IsNullOrWhiteSpace(expiresAt))
            message += $"\n\nEste Pix expira em {expiresAt}.";

        message += $"\n\nTXID PayFlow: {payment.TxId}";
        return message;
    }

    private static string? BuildCardWhatsappMessage(Payment payment, string customerName)
    {
        if (string.IsNullOrWhiteSpace(payment.AsaasInvoiceUrl))
            return null;

        var amount = payment.Amount.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        var dueDate = payment.DueDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

        var message = $"Olá, {customerName}. Sua cobrança PayFlow de {amount} vence em {dueDate}.";
        message += $"\n\nPague com cartão pelo link seguro:\n{payment.AsaasInvoiceUrl}";
        message += $"\n\nTXID PayFlow: {payment.TxId}";
        return message;
    }

    private static string? BuildWhatsappUrl(string customerPhone, string? message)
    {
        if (string.IsNullOrWhiteSpace(customerPhone) || string.IsNullOrWhiteSpace(message))
            return null;

        var digits = new string(customerPhone.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digits))
            return null;

        if (!digits.StartsWith("55") && (digits.Length == 10 || digits.Length == 11))
            digits = $"55{digits}";

        return $"https://wa.me/{digits}?text={Uri.EscapeDataString(message)}";
    }

    private static string NormalizePaymentMethod(string paymentMethod)
    {
        var normalized = paymentMethod.Trim().ToLowerInvariant();
        return normalized switch
        {
            "pix" => "pix",
            "card" => "card",
            "cartao" => "card",
            "cartão" => "card",
            "credito" => "card",
            "crédito" => "card",
            "debito" => "card",
            "débito" => "card",
            _ => throw new ValidationException("Forma de pagamento inválida. Use pix ou card.")
        };
    }

    private static string ToAsaasBillingType(string paymentMethod)
    {
        return paymentMethod == "pix" ? "PIX" : "CREDIT_CARD";
    }
}
