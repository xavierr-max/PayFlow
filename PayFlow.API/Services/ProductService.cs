using PayFlow.Domain.Sales.Entities;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;

namespace PayFlow.API.Services;

public interface IProductService
{
    ProductResponse CreateProduct(Guid sellerId, CreateProductRequest request);
    ProductResponse GetProduct(Guid id);
    List<ProductResponse> GetProductsBySeller(Guid sellerId);
    List<ProductResponse> GetProductsByCategory(Guid sellerId, string categoryId);
    ProductResponse UpdateProduct(Guid id, UpdateProductRequest request);
    void DeleteProduct(Guid id);
}

public class ProductService : IProductService
{
    private readonly IDataRepository _repository;

    public ProductService(IDataRepository repository)
    {
        _repository = repository;
    }

    public ProductResponse CreateProduct(Guid sellerId, CreateProductRequest request)
    {
        var product = new Product(request.Name, request.Description, request.Price, request.Quantity, sellerId);
        _repository.AddProduct(product);
        return MapToResponse(product, request.CategoryId, request.ImageUrl);
    }

    public ProductResponse GetProduct(Guid id)
    {
        var product = _repository.GetProduct(id);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found");

        return MapToResponse(product, "", "");
    }

    public List<ProductResponse> GetProductsBySeller(Guid sellerId)
    {
        var products = _repository.GetProductsBySeller(sellerId);
        return products.Select(p => MapToResponse(p, "", "")).ToList();
    }

    public List<ProductResponse> GetProductsByCategory(Guid sellerId, string categoryId)
    {
        var products = _repository.GetProductsBySeller(sellerId);
        return products
            .Select(p => MapToResponse(p, categoryId, ""))
            .Where(p => p.CategoryId == categoryId)
            .ToList();
    }

    public ProductResponse UpdateProduct(Guid id, UpdateProductRequest request)
    {
        var product = _repository.GetProduct(id);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found");

        product.Update(request.Name, request.Description, request.Price, request.Quantity);
        _repository.UpdateProduct(product);
        return MapToResponse(product, request.CategoryId, request.ImageUrl);
    }

    public void DeleteProduct(Guid id)
    {
        var product = _repository.GetProduct(id);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} not found");

        _repository.DeleteProduct(id);
    }

    private static ProductResponse MapToResponse(Product product, string categoryId, string imageUrl)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            SellerId = product.SellerId,
            CategoryId = categoryId,
            ImageUrl = imageUrl
        };
    }
}
