using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Sales.Entities;

namespace PayFlow.API.Services;

public interface ISellerService
{
    Task<SellerResponse> CreateSellerAsync(CreateSellerRequest request, CancellationToken cancellationToken);
    SellerResponse GetSeller(Guid id);
    Task<SellerResponse> UpdateSellerAsync(Guid id, UpdateSellerRequest request, CancellationToken cancellationToken);
    Task<SellerResponse> UpdateSellerAsaasSubaccountProfileAsync(Guid id, UpdateSellerAsaasSubaccountRequest request, CancellationToken cancellationToken);
    Task<SellerAsaasAccountResponse> CreateAsaasSubaccountAsync(Guid id, CancellationToken cancellationToken);
    AsaasAccountStatusResponse GetAsaasAccountStatus(Guid id);
    void DeleteSeller(Guid id);
}

public class SellerService : ISellerService
{
    private readonly IDataRepository _repository;
    private readonly IAsaasService _asaasService;
    private readonly INotificationService _notificationService;

    public SellerService(
        IDataRepository repository,
        IAsaasService asaasService,
        INotificationService notificationService)
    {
        _repository = repository;
        _asaasService = asaasService;
        _notificationService = notificationService;
    }

    public async Task<SellerResponse> CreateSellerAsync(
        CreateSellerRequest request,
        CancellationToken cancellationToken)
    {
        var seller = new Seller(request.Name, request.StoreName, request.PixKey ?? "");
        ApplyAsaasProfile(seller, request);

        _repository.AddSeller(seller);
        await TryCreateAsaasSubaccountIfPossibleAsync(seller, cancellationToken);
        return MapToResponse(seller);
    }

    public SellerResponse GetSeller(Guid id)
    {
        var seller = _repository.GetSeller(id);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {id} not found");

        return MapToResponse(seller);
    }

    public async Task<SellerResponse> UpdateSellerAsync(
        Guid id,
        UpdateSellerRequest request,
        CancellationToken cancellationToken)
    {
        var seller = GetSellerEntity(id);

        seller.Update(request.Name, request.StoreName, request.PixKey ?? "");
        if (HasAsaasProfilePayload(request))
            ApplyAsaasProfile(seller, request);

        _repository.UpdateSeller(seller);

        await TryCreateAsaasSubaccountIfPossibleAsync(seller, cancellationToken);
        return MapToResponse(seller);
    }

    public async Task<SellerResponse> UpdateSellerAsaasSubaccountProfileAsync(
        Guid id,
        UpdateSellerAsaasSubaccountRequest request,
        CancellationToken cancellationToken)
    {
        var seller = GetSellerEntity(id);

        ApplyAsaasProfile(seller, request);
        _repository.UpdateSeller(seller);

        await TryCreateAsaasSubaccountIfPossibleAsync(seller, cancellationToken);
        return MapToResponse(seller);
    }

    public async Task<SellerAsaasAccountResponse> CreateAsaasSubaccountAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var seller = GetSellerEntity(id);

        if (!seller.HasAsaasSubaccount())
            await CreateAsaasSubaccountOrThrowAsync(seller, cancellationToken);

        return new SellerAsaasAccountResponse
        {
            SellerId = seller.Id,
            AsaasAccountId = seller.AsaasAccountId!,
            AsaasWalletId = seller.AsaasWalletId!,
            Status = MapStatus(seller)
        };
    }

    public AsaasAccountStatusResponse GetAsaasAccountStatus(Guid id)
    {
        return MapStatus(GetSellerEntity(id));
    }

    public void DeleteSeller(Guid id)
    {
        var seller = _repository.GetSeller(id);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {id} not found");

        _repository.DeleteSeller(id);
    }

    private async Task TryCreateAsaasSubaccountIfPossibleAsync(
        Seller seller,
        CancellationToken cancellationToken)
    {
        if (seller.HasAsaasSubaccount() || !_asaasService.IsConfigured || !seller.HasRequiredAsaasSubaccountData())
            return;

        await CreateAsaasSubaccountOrThrowAsync(seller, cancellationToken);
    }

    private async Task CreateAsaasSubaccountOrThrowAsync(
        Seller seller,
        CancellationToken cancellationToken)
    {
        if (seller.HasAsaasSubaccount())
            return;

        var missing = seller.GetMissingAsaasSubaccountFields();
        if (missing.Count > 0)
        {
            throw new ValidationException(
                "Dados insuficientes para criar subconta Asaas: " + string.Join(", ", missing));
        }

        var result = await _asaasService.CreateSubaccountAsync(ToAsaasSubaccountRequest(seller), cancellationToken);
        seller.SetAsaasSubaccount(result.AccountId, result.WalletId, result.ApiKey);
        _repository.UpdateSeller(seller);

        _notificationService.NotifySeller(
            seller.Id,
            "asaas_subaccount_created",
            "Subconta Asaas criada",
            "A carteira Asaas do vendedor está pronta para receber Pix e cartão.",
            targetUrl: "/profile",
            deduplicationKey: $"asaas_subaccount_created:{seller.Id}:{result.AccountId}");
    }

    private static void ApplyAsaasProfile(Seller seller, CreateSellerRequest request)
    {
        seller.UpdateAsaasProfile(
            request.Email,
            request.CpfCnpj,
            request.MobilePhone,
            request.IncomeValue,
            request.Address,
            request.AddressNumber,
            request.Province,
            request.PostalCode,
            request.CompanyType,
            request.Phone,
            request.Complement,
            request.Site);
    }

    private static void ApplyAsaasProfile(Seller seller, UpdateSellerRequest request)
    {
        seller.UpdateAsaasProfile(
            request.Email,
            request.CpfCnpj,
            request.MobilePhone,
            request.IncomeValue,
            request.Address,
            request.AddressNumber,
            request.Province,
            request.PostalCode,
            request.CompanyType,
            request.Phone,
            request.Complement,
            request.Site);
    }

    private static void ApplyAsaasProfile(Seller seller, UpdateSellerAsaasSubaccountRequest request)
    {
        seller.UpdateAsaasProfile(
            request.Email,
            request.CpfCnpj,
            request.MobilePhone,
            request.IncomeValue,
            request.Address,
            request.AddressNumber,
            request.Province,
            request.PostalCode,
            request.CompanyType?.ToString(),
            seller.Phone,
            request.Complement,
            seller.Site);
    }

    private static bool HasAsaasProfilePayload(UpdateSellerRequest request)
    {
        return request.Email != null ||
               request.CpfCnpj != null ||
               request.MobilePhone != null ||
               request.Phone != null ||
               request.IncomeValue != null ||
               request.CompanyType != null ||
               request.Address != null ||
               request.AddressNumber != null ||
               request.Complement != null ||
               request.Province != null ||
               request.PostalCode != null ||
               request.Site != null;
    }

    private static AsaasSubaccountRequest ToAsaasSubaccountRequest(Seller seller)
    {
        return new AsaasSubaccountRequest(
            seller.StoreName,
            seller.Email!,
            seller.CpfCnpj!,
            seller.MobilePhone!,
            seller.IncomeValue!.Value,
            seller.Address!,
            seller.AddressNumber!,
            seller.Province!,
            seller.PostalCode!,
            seller.CompanyType,
            seller.Phone,
            seller.Complement,
            seller.Site);
    }

    private static SellerResponse MapToResponse(Seller seller)
    {
        return new SellerResponse
        {
            Id = seller.Id,
            Name = seller.Name,
            StoreName = seller.StoreName,
            PixKey = seller.PixKey,
            Email = seller.Email,
            CpfCnpj = seller.CpfCnpj,
            MobilePhone = seller.MobilePhone,
            Phone = seller.Phone,
            IncomeValue = seller.IncomeValue,
            CompanyType = seller.CompanyType,
            Address = seller.Address,
            AddressNumber = seller.AddressNumber,
            Complement = seller.Complement,
            Province = seller.Province,
            PostalCode = seller.PostalCode,
            Site = seller.Site,
            AsaasAccountId = seller.AsaasAccountId,
            AsaasWalletId = seller.AsaasWalletId,
            HasAsaasApiKey = !string.IsNullOrWhiteSpace(seller.AsaasApiKey),
            IsAsaasReady = seller.HasAsaasSubaccount(),
            AsaasSubaccountStatus = seller.AsaasSubaccountStatus,
            MissingAsaasFields = seller.GetMissingAsaasSubaccountFields().ToList()
        };
    }

    private static AsaasAccountStatusResponse MapStatus(Seller seller)
    {
        return new AsaasAccountStatusResponse
        {
            SellerId = seller.Id,
            AsaasAccountId = seller.AsaasAccountId,
            AsaasWalletId = seller.AsaasWalletId,
            HasApiKey = !string.IsNullOrWhiteSpace(seller.AsaasApiKey),
            IsReady = seller.HasAsaasSubaccount(),
            Status = seller.AsaasSubaccountStatus,
            MissingFields = seller.GetMissingAsaasSubaccountFields().ToList()
        };
    }

    private Seller GetSellerEntity(Guid id)
    {
        var seller = _repository.GetSeller(id);
        if (seller == null)
            throw new NotFoundException($"Seller with ID {id} not found");

        return seller;
    }
}
