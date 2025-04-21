using Microsoft.EntityFrameworkCore;
using MiniMart.API.Mappings;
using MiniMart.Application.Contracts;
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

//builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlite("Data Source=mini-mart-database.db;"));
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=mini-mart;trusted_connection=True"));
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

builder.Services.AddHttpClient<BankLinkService>(conf => conf.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI3MTNhZjkyNi1mY2JhLTRlNTYtOGU3My1lM2U5MDEzNDA5YzgiLCJNZXJjaGFudElkIjoiQ2h1a3MxMiIsImV4cCI6MTc3NjM5NDM2MywiaXNzIjoiaHR0cHM6Ly9jb3JhbHBheS5jb20iLCJhdWQiOiJodHRwczovL2NvcmFscGF5LmNvbSJ9.2vh4LBfC5-e5d6TZ3UOtcTwdjTI3l8QzqLEvMeo0J0Jz3Nb4OWwAIHiO8YeTiG49Knob1pmJuY_Gl7fy1VSNvQ"));


var app = builder.Build();

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