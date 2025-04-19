using System.ComponentModel.DataAnnotations;

namespace MiniMart.Domain.Models
{
    public class PurchaseOrder : EntityBase
    {
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; }
        public string CustomerId { get; set; }
        public DateTime Date { get; set; }
        public PurchaseStatus OrderStatus { get; set; }
        public string OrderData { get; set; }
        [Timestamp] // ensure optimistic concurrency control
        public byte[] RowVersion { get; set; }
    }
}
