namespace Endterm_IPT.Models
{
    public class Cart
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal BasePrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => BasePrice * Quantity;
    }
}
