using System.ComponentModel.DataAnnotations;

namespace Endterm_IPT.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public string ImageUrl { get; set; }

        public decimal PriceAtPurchase { get; set; }

        public decimal Subtotal => BasePrice * Quantity;

        public string FormattedPrice => BasePrice.ToString("C2");
        public string FormattedSubtotal => Subtotal.ToString("C2");
    }
}