using InventoryBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using InventoryBack.Application.Interfaces;
using InventoryBack.Infrastructure.Repositories;
using InventoryBack.Infrastructure.UnitOfWork;
using InventoryBack.Application.Services;
using InventoryBack.Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using InventoryBack.Application.Validators;
using InventoryBack.Infrastructure.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using InventoryBack.API.Middleware;
using InventoryBack.Infrastructure.Interfaces;
using QuestPDF.Infrastructure;

// Configure QuestPDF license (Community license for non-commercial use)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ========== CORS CONFIGURATION ==========
string[] allowedOrigins;

if (builder.Environment.IsDevelopment())
{
    // Development: Allow localhost ports (Vite, React, etc.)
    allowedOrigins = new[] { "http://localhost:3000" };
}
else
{
    // Production: Only allow your production frontend domain
    allowedOrigins = new[] { "https://mini-erp-frontend.vercel.app" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Enable if you need to send cookies/auth headers
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure camelCase for JSON serialization (matches frontend JS conventions)
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Database Context
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Generic Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));

// Domain-Specific Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductoBodegaRepository, ProductoBodegaRepository>();
builder.Services.AddScoped<ICampoExtraRepository, CampoExtraRepository>();
builder.Services.AddScoped<IBodegaRepository, BodegaRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IBodegaService, BodegaService>();
builder.Services.AddScoped<ICampoExtraService, CampoExtraService>();
builder.Services.AddScoped<IFacturaVentaService, FacturaVentaService>();
builder.Services.AddScoped<IFacturaCompraService, FacturaCompraService>();
builder.Services.AddScoped<IVendedorService, VendedorService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IMovimientoInventarioService, MovimientoInventarioService>();
builder.Services.AddScoped<ISkuGeneratorService, SkuGeneratorService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IDashboardService, DashboardService>(); // ?? Dashboard Analytics

// HttpClient for StorageService
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddProductoBodegaDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFacturaVentaDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFacturaCompraDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVendedorDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProveedorDtoValidator>();

// Infrastructure Services
builder.Services.AddScoped<IPdfGenerator, PdfGeneratorService>(); // ? NUEVO

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE ==========

// 1. Exception Handling (MUST BE FIRST)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. CORS (MUST BE BEFORE Authorization and MapControllers)
app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
// Enable Scalar in both Development and Production (Azure)
app.MapOpenApi();
app.MapScalarApiReference(options =>
    options.WithTitle("API de Inventario SCM")
    .WithTheme(ScalarTheme.DeepSpace)
    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Http));

// ========== HEALTH CHECK / PING ENDPOINTS ==========
// Azure App Service "Always On" needs a valid endpoint to ping

if (app.Environment.IsDevelopment())
{
    // Development: Detailed information for debugging
    app.MapGet("/", () => Results.Ok(new
    {
        service = "Inventory API",
        environment = "Development",
        status = "running",
        version = "1.0.0",
        timestamp = DateTime.UtcNow,
        endpoints = new
        {
            api = "/api",
            scalar = "/scalar/v1",
            openapi = "/openapi/v1.json",
            health = "/health"
        }
    }))
    .WithName("HealthCheck")
    .WithTags("Health")
    .Produces(200)
    .ExcludeFromDescription(); // Don't show in Swagger/Scalar
}
else
{
    // Production: Minimal response for security
    app.MapGet("/", () => Results.Ok(new
    {
        status = "ok",
        timestamp = DateTime.UtcNow
    }))
    .WithName("HealthCheck")
    .WithTags("Health")
    .Produces(200)
    .ExcludeFromDescription();
}

// Simple health check endpoint (same for all environments)
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}))
.WithName("Health")
.WithTags("Health")
.Produces(200);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
