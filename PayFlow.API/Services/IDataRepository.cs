using PayFlow.Domain.Sales.Entities;
using PayFlow.Domain.Billing.Entities;
using PayFlow.Domain.Sales.Enum;

namespace PayFlow.API.Services;

public interface IDataRepository
{
    // Sellers
    Seller? GetSeller(Guid id);
    void AddSeller(Seller seller);
    void UpdateSeller(Seller seller);
    void DeleteSeller(Guid id);

    // Products
    List<Product> GetProductsBySeller(Guid sellerId);
    Product? GetProduct(Guid id);
    void AddProduct(Product product);
    void UpdateProduct(Product product);
    void DeleteProduct(Guid id);

    // Customers
    List<Customer> GetCustomersBySeller(Guid sellerId);
    Customer? GetCustomer(Guid id);
    void AddCustomer(Customer customer);
    void UpdateCustomer(Customer customer);
    void DeleteCustomer(Guid id);

    // Payments
    List<Payment> GetPaymentsBySeller(Guid sellerId);
    List<Payment> GetPaymentsByStatus(Guid sellerId, PaymentStatus status);
    Payment? GetPayment(Guid id);
    void AddPayment(Payment payment);
    void UpdatePayment(Payment payment);
    void DeletePayment(Guid id);
}

public class InMemoryDataRepository : IDataRepository
{
    private List<Seller> _sellers = new();
    private List<Product> _products = new();
    private List<Customer> _customers = new();
    private List<Payment> _payments = new();

    public Seller? GetSeller(Guid id) => _sellers.FirstOrDefault(s => s.Id == id);
    public void AddSeller(Seller seller) => _sellers.Add(seller);
    public void UpdateSeller(Seller seller) { }
    public void DeleteSeller(Guid id) => _sellers.RemoveAll(s => s.Id == id);

    public List<Product> GetProductsBySeller(Guid sellerId) =>
        _products.Where(p => p.SellerId == sellerId).ToList();
    public Product? GetProduct(Guid id) => _products.FirstOrDefault(p => p.Id == id);
    public void AddProduct(Product product) => _products.Add(product);
    public void UpdateProduct(Product product) { }
    public void DeleteProduct(Guid id) => _products.RemoveAll(p => p.Id == id);

    public List<Customer> GetCustomersBySeller(Guid sellerId) =>
        _customers.Where(c => c.SellerId == sellerId).ToList();
    public Customer? GetCustomer(Guid id) => _customers.FirstOrDefault(c => c.Id == id);
    public void AddCustomer(Customer customer) => _customers.Add(customer);
    public void UpdateCustomer(Customer customer) { }
    public void DeleteCustomer(Guid id) => _customers.RemoveAll(c => c.Id == id);

    public List<Payment> GetPaymentsBySeller(Guid sellerId)
    {
        var customerIds = _customers.Where(c => c.SellerId == sellerId).Select(c => c.Id).ToList();
        return _payments.Where(p => customerIds.Contains(p.CustomerId)).ToList();
    }

    public List<Payment> GetPaymentsByStatus(Guid sellerId, PaymentStatus status)
    {
        var customerIds = _customers.Where(c => c.SellerId == sellerId).Select(c => c.Id).ToList();
        return _payments.Where(p => customerIds.Contains(p.CustomerId) && p.Status == status).ToList();
    }

    public Payment? GetPayment(Guid id) => _payments.FirstOrDefault(p => p.Id == id);
    public void AddPayment(Payment payment) => _payments.Add(payment);
    public void UpdatePayment(Payment payment) { }
    public void DeletePayment(Guid id) => _payments.RemoveAll(p => p.Id == id);
}
