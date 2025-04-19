namespace MiniMart.Domain.Models
{
    public class ProductInventory : EntityBase
    {
        public int ProductId { get; set; }  
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
