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

var builder = WebApplication.CreateBuilder(args);

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

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICampoExtraService, CampoExtraService>();
builder.Services.AddScoped<IBodegaService, BodegaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ISkuGeneratorService, SkuGeneratorService>();

// HttpClient for StorageService
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddProductoBodegaDtoValidator>();

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE ==========

// 1. Exception Handling (MUST BE FIRST)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
        options.WithTitle("API de Inventario SCM")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Http));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
