using PayFlow.Domain.Sales.Entities;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;

namespace PayFlow.API.Services;

public interface ISellerService
{
    SellerResponse CreateSeller(CreateSellerRequest request);
    SellerResponse GetSeller(Guid id);
    SellerResponse UpdateSeller(Guid id, UpdateSellerRequest request);
    void DeleteSeller(Guid id);
}

public class SellerService : ISellerService
{
    private readonly IDataRepository _repository;

    public SellerService(IDataRepository repository)
    {
        _repository = repository;
    }

    public SellerResponse CreateSeller(CreateSellerRequest request)
    {
        var seller = new Seller(request.Name, request.StoreName, request.PixKey);
        _repository.AddSeller(seller);
        return MapToResponse(seller);
    }

    public SellerResponse GetSeller(Guid id)
    {
        var seller = _repository.GetSeller(id);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {id} not found");

        return MapToResponse(seller);
    }

    public SellerResponse UpdateSeller(Guid id, UpdateSellerRequest request)
    {
        var seller = _repository.GetSeller(id);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {id} not found");

        seller.Update(request.Name, request.StoreName, request.PixKey, request.ConnectedAccountId);
        _repository.UpdateSeller(seller);
        return MapToResponse(seller);
    }

    public void DeleteSeller(Guid id)
    {
        var seller = _repository.GetSeller(id);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {id} not found");

        _repository.DeleteSeller(id);
    }

    private static SellerResponse MapToResponse(Seller seller)
    {
        return new SellerResponse
        {
            Id = seller.Id,
            Name = seller.Name,
            StoreName = seller.StoreName,
            PixKey = seller.PixKey
        };
    }
}
