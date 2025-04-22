using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.ActionFilters;
using MiniMart.API.Mappings;
using MiniMart.API.Middlewares;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;
using MiniMart.Application.Services;
using MiniMart.Infrastructure;
using MiniMart.Infrastructure.Repositories;
using MiniMart.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<WebhookActionFilter>();

builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IProductInventoryRepository, ProductInventoryRepository>();
builder.Services.AddScoped<IStockAlertRepository, StockAlertRepository>();
builder.Services.AddScoped<ITransactionQueryLogRepository, TransactionQueryLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IProductInventoryService, ProductInventoryService>();
builder.Services.AddScoped<IStockAlertService, StockAlertService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();

builder.Services.AddScoped<IExternalGatewayPaymentService, PayWithTransferService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHostedService<InventoryStockLevelMonitorService>();
builder.Services.AddHostedService<StockReconcilationService>();
builder.Services.AddHostedService<TransactionQueryProcessorService>();

builder.Services.Configure<BankLinkServiceConfig>(builder.Configuration.GetSection("BankLinkServiceConfig"));

builder.Services.AddHttpClient<BankLinkService>(conf => conf.DefaultRequestHeaders.Add("Authorization", builder.Configuration.GetSection("BankLinkServiceConfig:ApiToken").Value));

var app = builder.Build();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();