using System.ComponentModel.DataAnnotations;

namespace mcbaMVC.Models
{
    /// <summary>
    /// Stores login details for a customer, including a secure password hash.
    /// </summary>
    public class Login
    {
        [Key]
        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Login ID must be exactly 8 digits.")]
        public required string LoginID { get; set; }

        [Required(ErrorMessage = "Each login must be linked to a customer.")]
        public int CustomerID { get; set; }

        [Required, StringLength(94, ErrorMessage = "Password hash must be 94 characters.")]
        public required string PasswordHash { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
