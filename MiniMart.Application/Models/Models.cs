using MiniMart.Domain.Models;

namespace MiniMart.Application.Models
{
    public class ProductCreateRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductUpdateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductInventoryResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class ProductInventoryRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class PurchaseOrderStatusResponse
    {
        public bool isSuccessful { get; set; }
        public TransactionStatus Staus { get; set; }
        public string Message { get; set; }
    }
    public class PurchaseRequest
    {
        public string CustomerId { get; set; }
        public List<PurchaseItem> LineItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PurchaseItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class PurchaseResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string TrackingReference { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    public class PaymentRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentResponse
    {
        public string TrackingReference { get; set; }
        public bool IsSuccessful { get; set; }
        public string AccountNumber { get; set; }
        public DateTime ExpiryTime { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PurchaseOrderResponse
    {
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; }
        public string CustomerId { get; set; }
        public DateTime Date { get; set; }
        public string OrderStatus { get; set; }
    }

    public class QueryTransactionRequest
    {
        public string TransactionId { get; set; }
    }

    public class QueryTransactionResponse
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public decimal AmountProcessed { get; set; }
        public bool ShouldRequery { get; set; }
    }

    public class WebhookRequest
    {
        public string RefId { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class WebhookResponse
    {
        public string Message { get; set; }
    }
}
