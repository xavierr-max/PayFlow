using PayFlow.Domain.Billing.Entities;
using PayFlow.Domain.Sales.Enum;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Sales.Entities;
using System.Globalization;

namespace PayFlow.API.Services;

public interface IPaymentService
{
    PaymentResponse CreatePayment(Guid sellerId, CreatePaymentRequest request);
    PaymentResponse GetPayment(Guid id);
    List<PaymentResponse> GetPaymentsBySeller(Guid sellerId);
    List<PaymentResponse> GetPaymentsByStatus(Guid sellerId, string status);
    PaymentResponse MarkAsPaid(Guid id, MarkPaymentPaidRequest request);
    Task<PaymentResponse> CreatePixWhatsappMessage(Guid id, CancellationToken cancellationToken);
    Task<PaymentResponse> CreateWhatsappPaymentMessage(Guid id, CreatePaymentWhatsappMessageRequest? request, CancellationToken cancellationToken);
    Task<PaymentResponse> RefreshStripePaymentStatus(Guid id, CancellationToken cancellationToken);
    PaymentResponse SyncStripePaymentIntentStatus(Guid id, string stripePaymentIntentId, string stripePaymentStatus);
    PaymentResponse SyncStripeCheckoutSessionStatus(Guid id, string checkoutSessionId, string? paymentIntentId, string paymentStatus);
    void DeletePayment(Guid id);
}

public class PaymentService : IPaymentService
{
    private readonly IDataRepository _repository;
    private readonly ICustomerService _customerService;
    private readonly IStripePixService _stripePixService;
    private readonly IStripeCheckoutService _stripeCheckoutService;
    private readonly IStripePaymentStatusService _stripePaymentStatusService;

    public PaymentService(
        IDataRepository repository,
        ICustomerService customerService,
        IStripePixService stripePixService,
        IStripeCheckoutService stripeCheckoutService,
        IStripePaymentStatusService stripePaymentStatusService)
    {
        _repository = repository;
        _customerService = customerService;
        _stripePixService = stripePixService;
        _stripeCheckoutService = stripeCheckoutService;
        _stripePaymentStatusService = stripePaymentStatusService;
    }

    public PaymentResponse CreatePayment(Guid sellerId, CreatePaymentRequest request)
    {
        var customer = _repository.GetCustomer(request.CustomerId);
        if (customer == null || customer.SellerId != sellerId)
            throw new NotFoundException($"Customer with ID {request.CustomerId} not found for this seller");

        var payment = new Payment(request.DueDate, request.Amount, request.InstallmentNumber, request.TotalInstallments, request.CustomerId);
        var items = BuildPaymentItems(payment, sellerId, request.Items);

        _repository.AddPayment(payment);

        if (items.Count > 0)
            _repository.AddPaymentItems(items);

        return MapToResponse(payment, customer);
    }

    public PaymentResponse GetPayment(Guid id)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer);
    }

    public List<PaymentResponse> GetPaymentsBySeller(Guid sellerId)
    {
        var payments = _repository.GetPaymentsBySeller(sellerId);
        return payments.Select(p =>
        {
            var customer = _repository.GetCustomer(p.CustomerId);
            return MapToResponse(p, customer);
        }).ToList();
    }

    public List<PaymentResponse> GetPaymentsByStatus(Guid sellerId, string status)
    {
        if (!Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            throw new ValidationException($"Invalid payment status: {status}");

        var payments = _repository.GetPaymentsByStatus(sellerId, paymentStatus);
        return payments.Select(p =>
        {
            var customer = _repository.GetCustomer(p.CustomerId);
            return MapToResponse(p, customer);
        }).ToList();
    }

    public PaymentResponse MarkAsPaid(Guid id, MarkPaymentPaidRequest request)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        payment.MarkAsPaid(request.ValueReceived, request.PaidAt);
        DeductStockIfPaid(payment);
        _repository.UpdatePayment(payment);

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer);
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
        CreatePaymentWhatsappMessageRequest? request,
        CancellationToken cancellationToken)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        if (payment.Status == PaymentStatus.Paid)
            throw new ValidationException("Não é possível gerar link para uma cobrança já paga");

        var customer = _repository.GetCustomer(payment.CustomerId);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {payment.CustomerId} not found");

        var storeName = GetStoreName(customer);
        var paymentMethod = NormalizePaymentMethod(request?.PaymentMethod);

        if (paymentMethod == "pix")
        {
            if (!payment.HasActivePix(DateTime.UtcNow))
            {
                var pix = await _stripePixService.CreatePixAsync(payment, customer, storeName, cancellationToken);
                payment.AttachStripePix(
                    pix.PaymentIntentId,
                    pix.PaymentStatus,
                    pix.CopyPaste,
                    pix.QrCodeImageUrl,
                    pix.QrCodeSvgUrl,
                    pix.HostedInstructionsUrl,
                    pix.ExpiresAt);
            }

            payment.MarkPixMessageSent();
        }
        else
        {
            if (!payment.HasActiveCheckoutSession(DateTime.UtcNow, paymentMethod))
            {
                var checkout = await _stripeCheckoutService.CreateCardCheckoutSessionAsync(payment, customer, storeName, cancellationToken);
                payment.AttachStripeCheckoutSession(
                    checkout.SessionId,
                    checkout.CheckoutUrl,
                    checkout.ExpiresAt,
                    paymentMethod,
                    checkout.PaymentIntentId);
            }

            payment.MarkPaymentMessageSent();
        }

        _repository.UpdatePayment(payment);

        return MapToResponse(payment, customer);
    }

    public async Task<PaymentResponse> RefreshStripePaymentStatus(Guid id, CancellationToken cancellationToken)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        var stripeStatus = await _stripePaymentStatusService.GetStatusAsync(payment, cancellationToken);

        if (!string.IsNullOrWhiteSpace(stripeStatus.PaymentIntentId))
            payment.AttachStripePaymentIntent(stripeStatus.PaymentIntentId);

        payment.SyncStripePaymentStatus(stripeStatus.PaymentStatus);
        DeductStockIfPaid(payment);
        _repository.UpdatePayment(payment);

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer);
    }

    public PaymentResponse SyncStripePaymentIntentStatus(Guid id, string stripePaymentIntentId, string stripePaymentStatus)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        if (!string.IsNullOrWhiteSpace(payment.StripePaymentIntentId) &&
            !payment.StripePaymentIntentId.Equals(stripePaymentIntentId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Stripe payment intent does not match this payment");
        }

        payment.SyncStripePaymentStatus(stripePaymentStatus);
        DeductStockIfPaid(payment);
        _repository.UpdatePayment(payment);

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer);
    }

    public PaymentResponse SyncStripeCheckoutSessionStatus(
        Guid id,
        string checkoutSessionId,
        string? paymentIntentId,
        string paymentStatus)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        if (!string.IsNullOrWhiteSpace(payment.StripeCheckoutSessionId) &&
            !payment.StripeCheckoutSessionId.Equals(checkoutSessionId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Stripe checkout session does not match this payment");
        }

        payment.AttachStripePaymentIntent(paymentIntentId ?? "");
        payment.SyncStripePaymentStatus(paymentStatus);
        DeductStockIfPaid(payment);
        _repository.UpdatePayment(payment);

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer);
    }

    public void DeletePayment(Guid id)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        _repository.DeletePayment(id);
    }

    private static string GetStatusLabel(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "Pendente",
            PaymentStatus.Paid => "Pago",
            PaymentStatus.Overdue => "Atrasado",
            _ => "Desconhecido"
        };
    }

    private List<PaymentItem> BuildPaymentItems(Payment payment, Guid sellerId, List<CreatePaymentItemRequest>? requestItems)
    {
        if (requestItems == null || requestItems.Count == 0)
            return new List<PaymentItem>();

        return requestItems
            .Where(item => item.ProductId != Guid.Empty && item.Quantity > 0)
            .Select(item =>
            {
                var product = _repository.GetProduct(item.ProductId);
                if (product == null || product.SellerId != sellerId)
                    throw new ValidationException("Produto inválido para esta venda");

                if (product.Quantity < item.Quantity)
                    throw new ValidationException($"Estoque insuficiente para {product.Name}");

                var unitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.Price;
                return new PaymentItem(product.Id, payment.Id, item.Quantity, unitPrice);
            })
            .ToList();
    }

    private void DeductStockIfPaid(Payment payment)
    {
        if (payment.Status != PaymentStatus.Paid || payment.StockDeductedAt.HasValue)
            return;

        var items = _repository.GetPaymentItems(payment.Id);
        if (items.Count == 0)
            return;

        foreach (var item in items)
        {
            var product = _repository.GetProduct(item.ProductId);
            if (product == null)
                continue;

            product.DecreaseStock(item.Quantity);
            _repository.UpdateProduct(product);
        }

        payment.MarkStockDeducted();
    }

    private PaymentResponse MapToResponse(Payment payment, Customer? customer)
    {
        return MapToResponse(
            payment,
            customer?.Name ?? "",
            customer?.Phone ?? "",
            GetStoreName(customer));
    }

    private string GetStoreName(Customer? customer)
    {
        if (customer == null)
            return "PayFlow";

        var seller = _repository.GetSeller(customer.SellerId);
        return string.IsNullOrWhiteSpace(seller?.StoreName) ? "PayFlow" : seller.StoreName;
    }

    private static PaymentResponse MapToResponse(Payment payment, string customerName, string customerPhone, string storeName)
    {
        var whatsappMessage = BuildWhatsappMessage(payment, customerName, storeName);
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
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            StripePaymentIntentId = payment.StripePaymentIntentId,
            StripePaymentStatus = payment.StripePaymentStatus,
            PixCopyPaste = payment.PixCopyPaste,
            PixQrCodeImageUrl = payment.PixQrCodeImageUrl,
            PixQrCodeSvgUrl = payment.PixQrCodeSvgUrl,
            PixHostedInstructionsUrl = payment.PixHostedInstructionsUrl,
            PixExpiresAt = payment.PixExpiresAt,
            PixMessageSentAt = payment.PixMessageSentAt,
            StripePaymentMethod = payment.StripePaymentMethod,
            StripeCheckoutSessionId = payment.StripeCheckoutSessionId,
            StripeCheckoutUrl = payment.StripeCheckoutUrl,
            StripeCheckoutUrlExpiresAt = payment.StripeCheckoutUrlExpiresAt,
            PaymentMessageSentAt = payment.PaymentMessageSentAt,
            WhatsappMessage = whatsappMessage,
            WhatsappUrl = whatsappUrl
        };
    }

    private static string? BuildWhatsappMessage(Payment payment, string customerName, string storeName)
    {
        if (payment.StripePaymentMethod?.Equals("card", StringComparison.OrdinalIgnoreCase) == true)
            return BuildCardWhatsappMessage(payment, customerName, storeName);

        if (string.IsNullOrWhiteSpace(payment.PixCopyPaste) && string.IsNullOrWhiteSpace(payment.PixHostedInstructionsUrl))
            return null;

        var amount = payment.Amount.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        var dueDate = payment.DueDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));
        var expiresAt = payment.PixExpiresAt?.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR"));

        var message = $"Olá, {customerName}. Sua cobrança da {storeName} de {amount} vence em {dueDate}.";
        message += $"\n\nParcela {payment.InstallmentNumber}/{payment.TotalInstallments}.";

        if (!string.IsNullOrWhiteSpace(payment.PixCopyPaste))
            message += $"\n\nPix copia e cola:\n{payment.PixCopyPaste}";

        if (!string.IsNullOrWhiteSpace(payment.PixHostedInstructionsUrl))
            message += $"\n\nLink com QR Code:\n{payment.PixHostedInstructionsUrl}";

        if (!string.IsNullOrWhiteSpace(expiresAt))
            message += $"\n\nEste Pix expira em {expiresAt}.";

        message += $"\n\nTXID PayFlow: {payment.TxId}";
        return message;
    }

    private static string? BuildCardWhatsappMessage(Payment payment, string customerName, string storeName)
    {
        if (string.IsNullOrWhiteSpace(payment.StripeCheckoutUrl))
            return null;

        var amount = payment.Amount.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        var dueDate = payment.DueDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));
        var expiresAt = payment.StripeCheckoutUrlExpiresAt?.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR"));

        var message = $"Olá, {customerName}. Sua cobrança da {storeName} de {amount} vence em {dueDate}.";
        message += $"\n\nParcela {payment.InstallmentNumber}/{payment.TotalInstallments}.";
        message += $"\n\nPague com cartão de crédito ou débito pelo link seguro da Stripe:\n{payment.StripeCheckoutUrl}";

        if (!string.IsNullOrWhiteSpace(expiresAt))
            message += $"\n\nEste link expira em {expiresAt}.";

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

    private static string NormalizePaymentMethod(string? paymentMethod)
    {
        var normalized = paymentMethod?.Trim().ToLowerInvariant();
        return normalized switch
        {
            null or "" => "pix",
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
}
