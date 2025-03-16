using ExtensionMarket;
using ExtensionMarket.Data;
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;

using ExtensionMarket.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

#if DEBUG
using ExtensionMarket.Security;
using Scalar.AspNetCore;
#endif

var builder = WebApplication.CreateBuilder(args);

// Initialize configurations from environment variables
// Database connection
Configurations.DbConnectionString = Environment.GetEnvironmentVariable("DBCONNECTION") ??
    builder.Configuration.GetConnectionString("DefaultConnection")!;

// File storage
Configurations.FileStorageBasePath = Environment.GetEnvironmentVariable("FILESTORAGE_BASEPATH") ??
    builder.Configuration["FileStorage:BasePath"] ?? Path.Combine(AppContext.BaseDirectory, "Files");

// JWT settings
Configurations.JwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ??
    builder.Configuration["JWT:Secret"]!;
Configurations.JwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ??
    builder.Configuration["JWT:ValidIssuer"] ?? Constants.ExtensionMarket;
Configurations.JwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
    builder.Configuration["JWT:ValidAudience"] ?? Constants.ExtensionMarket;
Configurations.JwtTokenValidityInMinutes = int.Parse(
    Environment.GetEnvironmentVariable("JWT_TOKEN_VALIDITY_MINUTES") ??
    builder.Configuration["JWT:TokenValidityInMinutes"] ?? "60");

builder.Services.Configure<FormOptions>(
    option =>
    {
        option.ValueLengthLimit = int.MaxValue;
        option.MultipartBodyLengthLimit = int.MaxValue;
        option.MultipartHeadersLengthLimit = int.MaxValue;
    }
);
builder.WebHost.ConfigureKestrel(option => { option.Limits.MaxRequestBodySize = int.MaxValue; });

// Add services to the container.
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseNpgsql(Configurations.DbConnectionString));

// Register services
builder.Services.AddScoped<IExtensionService, ExtensionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddControllers();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Configurations.JwtIssuer,
        ValidAudience = Configurations.JwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configurations.JwtKey)),
    };
});

// Configure OpenAPI with Scalar
#if DEBUG
builder.Services.AddOpenApi(opt =>
{
    opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
#endif

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsPolicyBuilder =>
    {
        corsPolicyBuilder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

#if DEBUG
if (Environment.GetEnvironmentVariable("EXTENSIONMARKET_MIGRATION") is not null)
{
    Console.Out.WriteLine("警告: 该操作将重置所有数据库设置，确认进行吗? 输入 ExtensionMarket 以确定(区分大小写)");
    if (Console.ReadLine() is "ExtensionMarket")
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        var userService = services.GetRequiredService<IUserService>();
        await userService.CreateUserAsync(
            username: "admin",
            email: "admin@example.com",
            password: "Admin@123",
            roles: [UserRole.Admin, UserRole.Developer]
        );
    }
    else
    {
        Console.WriteLine("迁移取消");
    }
}

app.UseDeveloperExceptionPage();
app.MapOpenApi();
app.MapScalarApiReference(option => { option.Servers = [new ScalarServer("http://localhost:5271")]; });
#endif

#if RELEASE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}
#endif

app.UseCors("AllowAll");

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure the file storage directory exists
if (!Directory.Exists(Configurations.FileStorageBasePath))
{
    Directory.CreateDirectory(Configurations.FileStorageBasePath);
}

app.Run();
