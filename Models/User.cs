using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Endterm_IPT.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(255)]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [MaxLength(50)]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        [MaxLength(20)]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(20)]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        public bool IsAdmin { get; set; } = false;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}
