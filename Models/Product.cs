namespace Endterm_IPT.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
    }
}
