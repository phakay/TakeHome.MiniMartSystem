namespace MiniMart.Domain.Models
{
    public class Product : EntityBase
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
