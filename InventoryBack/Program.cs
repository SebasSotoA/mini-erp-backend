using InventoryBack.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<InventoryDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

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
