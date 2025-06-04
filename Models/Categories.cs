using System.ComponentModel.DataAnnotations;

namespace Endterm_IPT.Models
{
    public class Categories
    {

        public int categoryId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string categoryName { get; set; }

        public string? categoryDescription { get; set; }
    }
}
