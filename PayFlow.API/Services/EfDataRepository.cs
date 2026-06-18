using Microsoft.EntityFrameworkCore;
using PayFlow.API.Data;
using PayFlow.Domain.Sales.Entities;
using PayFlow.Domain.Billing.Entities;
using PayFlow.Domain.Sales.Enum;

namespace PayFlow.API.Services;

public class EfDataRepository : IDataRepository
{
    private readonly AppDbContext _context;

    public EfDataRepository(AppDbContext context)
    {
        _context = context;
    }

    // Sellers
    public Seller? GetSeller(Guid id) => _context.Sellers.Find(id);
    public void AddSeller(Seller seller)
    {
        _context.Sellers.Add(seller);
        _context.SaveChanges();
    }
    public void UpdateSeller(Seller seller)
    {
        _context.Sellers.Update(seller);
        _context.SaveChanges();
    }
    public void DeleteSeller(Guid id)
    {
        var entity = _context.Sellers.Find(id);
        if (entity != null)
        {
            _context.Sellers.Remove(entity);
            _context.SaveChanges();
        }
    }

    // Categories
    public List<Category> GetCategories() => _context.Categories.OrderBy(c => c.Name).ToList();
    public Category? GetCategory(Guid id) => _context.Categories.Find(id);
    public void AddCategory(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();
    }
    public void UpdateCategory(Category category)
    {
        _context.Categories.Update(category);
        _context.SaveChanges();
    }

    // Products
    public List<Product> GetProductsBySeller(Guid sellerId) => _context.Products.Where(p => p.SellerId == sellerId).ToList();
    public Product? GetProduct(Guid id) => _context.Products.Find(id);
    public void AddProduct(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }
    public void UpdateProduct(Product product)
    {
        _context.Products.Update(product);
        _context.SaveChanges();
    }
    public void DeleteProduct(Guid id)
    {
        var entity = _context.Products.Find(id);
        if (entity != null)
        {
            _context.Products.Remove(entity);
            _context.SaveChanges();
        }
    }

    // Customers
    public List<Customer> GetCustomersBySeller(Guid sellerId) => _context.Customers.Where(c => c.SellerId == sellerId).ToList();
    public Customer? GetCustomer(Guid id) => _context.Customers.Find(id);
    public void AddCustomer(Customer customer)
    {
        _context.Customers.Add(customer);
        _context.SaveChanges();
    }
    public void UpdateCustomer(Customer customer)
    {
        _context.Customers.Update(customer);
        _context.SaveChanges();
    }
    public void DeleteCustomer(Guid id)
    {
        var entity = _context.Customers.Find(id);
        if (entity != null)
        {
            _context.Customers.Remove(entity);
            _context.SaveChanges();
        }
    }

    // Payments
    public List<Payment> GetPaymentsBySeller(Guid sellerId)
    {
        return _context.Payments.Where(p => p.SellerId == sellerId).ToList();
    }

    public List<Payment> GetPaymentsByStatus(Guid sellerId, PaymentStatus status)
    {
        return _context.Payments.Where(p => p.SellerId == sellerId && p.Status == status).ToList();
    }

    public Payment? GetPayment(Guid id) => _context.Payments.Find(id);
    public Payment? GetPaymentByAsaasPaymentId(string asaasPaymentId) =>
        _context.Payments.FirstOrDefault(p => p.AsaasPaymentId == asaasPaymentId);

    public Payment? GetPaymentByTxId(string txId) =>
        _context.Payments.FirstOrDefault(p => p.TxId == txId);

    public void AddPayment(Payment payment)
    {
        _context.Payments.Add(payment);
        _context.SaveChanges();
    }
    public void UpdatePayment(Payment payment)
    {
        _context.Payments.Update(payment);
        _context.SaveChanges();
    }
    public void DeletePayment(Guid id)
    {
        var entity = _context.Payments.Find(id);
        if (entity != null)
        {
            _context.Payments.Remove(entity);
            _context.SaveChanges();
        }
    }

    public List<PaymentItem> GetPaymentItems(Guid paymentId) =>
        _context.PaymentItems.Where(item => item.PaymentId == paymentId).ToList();

    public void AddPaymentItems(List<PaymentItem> items)
    {
        _context.PaymentItems.AddRange(items);
        _context.SaveChanges();
    }

    public List<PaymentTransaction> GetPaymentTransactions(Guid paymentId) =>
        _context.PaymentTransactions
            .Where(transaction => transaction.PaymentId == paymentId)
            .OrderByDescending(transaction => transaction.OccurredAt)
            .ToList();

    public void AddPaymentTransaction(PaymentTransaction transaction)
    {
        _context.PaymentTransactions.Add(transaction);
        _context.SaveChanges();
    }

    public WebhookEventLog? GetWebhookEventLog(string provider, string eventId) =>
        _context.WebhookEventLogs.FirstOrDefault(log => log.Provider == provider && log.EventId == eventId);

    public void AddWebhookEventLog(WebhookEventLog webhookEventLog)
    {
        _context.WebhookEventLogs.Add(webhookEventLog);
        _context.SaveChanges();
    }

    public void UpdateWebhookEventLog(WebhookEventLog webhookEventLog)
    {
        _context.WebhookEventLogs.Update(webhookEventLog);
        _context.SaveChanges();
    }
}
