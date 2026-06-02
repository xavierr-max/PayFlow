using PayFlow.Domain.Sales.Entities;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;

namespace PayFlow.API.Services;

public interface ICustomerService
{
    CustomerResponse CreateCustomer(Guid sellerId, CreateCustomerRequest request);
    CustomerResponse GetCustomer(Guid id);
    List<CustomerResponse> GetCustomersBySeller(Guid sellerId);
    CustomerResponse UpdateCustomer(Guid id, UpdateCustomerRequest request);
    void DeleteCustomer(Guid id);
}

public class CustomerService : ICustomerService
{
    private readonly IDataRepository _repository;

    public CustomerService(IDataRepository repository)
    {
        _repository = repository;
    }

    public CustomerResponse CreateCustomer(Guid sellerId, CreateCustomerRequest request)
    {
        var customer = new Customer(request.Name, request.Description, request.Phone, sellerId);
        customer.UpdateAsaasProfile(request.CpfCnpj, request.Email);
        _repository.AddCustomer(customer);
        return MapToResponse(customer);
    }

    public CustomerResponse GetCustomer(Guid id)
    {
        var customer = _repository.GetCustomer(id);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {id} not found");

        return MapToResponse(customer);
    }

    public List<CustomerResponse> GetCustomersBySeller(Guid sellerId)
    {
        var customers = _repository.GetCustomersBySeller(sellerId);
        return customers.Select(MapToResponse).ToList();
    }

    public CustomerResponse UpdateCustomer(Guid id, UpdateCustomerRequest request)
    {
        var customer = _repository.GetCustomer(id);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {id} not found");

        customer.Update(request.Name, request.Description, request.Phone);
        customer.UpdateAsaasProfile(request.CpfCnpj, request.Email);
        _repository.UpdateCustomer(customer);
        return MapToResponse(customer);
    }

    public void DeleteCustomer(Guid id)
    {
        var customer = _repository.GetCustomer(id);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {id} not found");

        _repository.DeleteCustomer(id);
    }

    private static CustomerResponse MapToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Description = customer.Description,
            Phone = customer.Phone,
            CpfCnpj = customer.CpfCnpj,
            Email = customer.Email,
            AsaasCustomerId = customer.AsaasCustomerId,
            SellerId = customer.SellerId
        };
    }
}
