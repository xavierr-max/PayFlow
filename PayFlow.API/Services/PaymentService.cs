using PayFlow.Domain.Billing.Entities;
using PayFlow.Domain.Sales.Enum;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;

namespace PayFlow.API.Services;

public interface IPaymentService
{
    PaymentResponse CreatePayment(Guid sellerId, CreatePaymentRequest request);
    PaymentResponse GetPayment(Guid id);
    List<PaymentResponse> GetPaymentsBySeller(Guid sellerId);
    List<PaymentResponse> GetPaymentsByStatus(Guid sellerId, string status);
    PaymentResponse MarkAsPaid(Guid id, MarkPaymentPaidRequest request);
    void DeletePayment(Guid id);
}

public class PaymentService : IPaymentService
{
    private readonly IDataRepository _repository;
    private readonly ICustomerService _customerService;

    public PaymentService(IDataRepository repository, ICustomerService customerService)
    {
        _repository = repository;
        _customerService = customerService;
    }

    public PaymentResponse CreatePayment(Guid sellerId, CreatePaymentRequest request)
    {
        var customer = _repository.GetCustomer(request.CustomerId);
        if (customer == null || customer.SellerId != sellerId)
            throw new NotFoundException($"Customer with ID {request.CustomerId} not found for this seller");

        var payment = new Payment(request.DueDate, request.Amount, request.InstallmentNumber, request.TotalInstallments, request.CustomerId);
        _repository.AddPayment(payment);
        return MapToResponse(payment, customer.Name);
    }

    public PaymentResponse GetPayment(Guid id)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "");
    }

    public List<PaymentResponse> GetPaymentsBySeller(Guid sellerId)
    {
        var payments = _repository.GetPaymentsBySeller(sellerId);
        return payments.Select(p =>
        {
            var customer = _repository.GetCustomer(p.CustomerId);
            return MapToResponse(p, customer?.Name ?? "");
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
            return MapToResponse(p, customer?.Name ?? "");
        }).ToList();
    }

    public PaymentResponse MarkAsPaid(Guid id, MarkPaymentPaidRequest request)
    {
        var payment = _repository.GetPayment(id);
        if (payment == null)
            throw new NotFoundException($"Payment with ID {id} not found");

        payment.MarkAsPaid(request.ValueReceived);
        _repository.UpdatePayment(payment);

        var customer = _repository.GetCustomer(payment.CustomerId);
        return MapToResponse(payment, customer?.Name ?? "");
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

    private static PaymentResponse MapToResponse(Payment payment, string customerName)
    {
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
            CustomerName = customerName
        };
    }
}
