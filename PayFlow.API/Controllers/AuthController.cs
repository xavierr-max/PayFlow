using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PayFlow.API.Auth;
using PayFlow.API.Data;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;
using PayFlow.API.Services;
using PayFlow.Domain.Sales.Entities;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly IAsaasService _asaasService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        IAsaasService asaasService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
        _asaasService = asaasService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        ValidateRegisterRequest(request);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ValidationException("E-mail já cadastrado");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var seller = new Seller(request.Name, request.StoreName, request.PixKey ?? "");
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
        _dbContext.Sellers.Add(seller);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            SellerId = seller.Id,
            IsPremium = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ValidationException(string.Join("; ", result.Errors.Select(error => error.Description)));

        await TryCreateAsaasSubaccountAsync(seller, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return Ok(CreateAuthResponse(user));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("E-mail ou senha inválidos");

        return Ok(CreateAuthResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserResponse>> Me()
    {
        var user = await GetCurrentUser();
        return Ok(MapUser(user));
    }

    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ValidationException("Informe a senha atual e a nova senha");

        var user = await GetCurrentUser();
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationException(string.Join("; ", result.Errors.Select(error => error.Description)));

        return NoContent();
    }

    [Authorize]
    [HttpPut("subscription")]
    public async Task<ActionResult<AuthResponse>> UpdateSubscription([FromBody] UpdateSubscriptionRequest request)
    {
        await Task.CompletedTask;
        throw new ValidationException("Assinatura Premium deve ser gerenciada pelo checkout Asaas");
    }

    private async Task TryCreateAsaasSubaccountAsync(Seller seller, CancellationToken cancellationToken)
    {
        if (!_asaasService.IsConfigured || !seller.HasRequiredAsaasSubaccountData())
            return;

        var result = await _asaasService.CreateSubaccountAsync(
            new AsaasSubaccountRequest(
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
                seller.Site),
            cancellationToken);

        seller.SetAsaasSubaccount(result.AccountId, result.WalletId, result.ApiKey);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ApplicationUser> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var parsedUserId))
            throw new UnauthorizedException("Usuário não autenticado");

        var user = await _userManager.Users.FirstOrDefaultAsync(item => item.Id == parsedUserId);
        if (user == null)
            throw new UnauthorizedException("Usuário não encontrado");

        return user;
    }

    private AuthResponse CreateAuthResponse(ApplicationUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("seller_id", user.SellerId.ToString()),
            new("is_premium", user.IsPremium ? "true" : "false")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            User = MapUser(user)
        };
    }

    private static AuthUserResponse MapUser(ApplicationUser user)
    {
        return new AuthUserResponse
        {
            Id = user.Id,
            Email = user.Email ?? "",
            SellerId = user.SellerId,
            IsPremium = user.IsPremium
        };
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Informe o e-mail");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ValidationException("A senha deve ter pelo menos 6 caracteres");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Informe o nome do responsável");

        if (string.IsNullOrWhiteSpace(request.StoreName))
            throw new ValidationException("Informe o nome da loja");
    }
}
