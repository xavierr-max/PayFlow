using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PayFlow.API.Auth;
using PayFlow.API.Services;
using PayFlow.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PayFlow.API.Data;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.Configure<AsaasOptions>(builder.Configuration.GetSection("Asaas"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var jwtKey = Encoding.UTF8.GetBytes(jwtOptions.Key);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Premium", policy => policy.RequireClaim("is_premium", "true"));
});

builder.Services.AddScoped<IDataRepository, EfDataRepository>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPremiumSubscriptionService, PremiumSubscriptionService>();
builder.Services.AddScoped<IAsaasService, AsaasService>();
builder.Services.AddScoped<IAsaasWebhookService, AsaasWebhookService>();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Use middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoints
app.MapGet("/", () => "PayFlow API - Ready to accept requests from frontend");
app.MapGet("/api/health", () => new { status = "ok", timestamp = DateTime.UtcNow });

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true);
    var requireDatabase = app.Configuration.GetValue("Database:RequireOnStartup", !app.Environment.IsDevelopment());

    if (applyMigrations)
    {
        var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

        if (!requireDatabase && !IsPostgresPortReachable(connectionString))
        {
            logger.LogWarning(
                "PostgreSQL não está acessível em {Target}. A API continuará em modo de desenvolvimento, mas endpoints que usam banco falharão até o banco estar online.",
                GetPostgresConnectionTarget(connectionString));
        }
        else
        {
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
            catch (Exception ex) when (!requireDatabase)
            {
                logger.LogWarning(
                    "Não foi possível aplicar migrations no PostgreSQL. A API continuará em modo de desenvolvimento. Motivo: {Message}",
                    ex.Message);
            }
        }
    }
}

app.Run();

static bool IsPostgresPortReachable(string? connectionString)
{
    try
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var host = GetFirstHost(builder.Host);
        using var client = new TcpClient();
        return client.ConnectAsync(host, builder.Port).Wait(TimeSpan.FromMilliseconds(500)) && client.Connected;
    }
    catch
    {
        return false;
    }
}

static string GetPostgresConnectionTarget(string? connectionString)
{
    try
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"{GetFirstHost(builder.Host)}:{builder.Port}";
    }
    catch
    {
        return "PostgreSQL configurado";
    }
}

static string GetFirstHost(string? host)
{
    return string.IsNullOrWhiteSpace(host)
        ? "localhost"
        : host.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "localhost";
}
