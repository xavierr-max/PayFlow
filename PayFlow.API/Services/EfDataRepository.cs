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
        var customerIds = _context.Customers.Where(c => c.SellerId == sellerId).Select(c => c.Id).ToList();
        return _context.Payments.Where(p => customerIds.Contains(p.CustomerId)).ToList();
    }

    public List<Payment> GetPaymentsByStatus(Guid sellerId, PaymentStatus status)
    {
        var customerIds = _context.Customers.Where(c => c.SellerId == sellerId).Select(c => c.Id).ToList();
        return _context.Payments.Where(p => customerIds.Contains(p.CustomerId) && p.Status == status).ToList();
    }

    public Payment? GetPayment(Guid id) => _context.Payments.Find(id);
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
}
