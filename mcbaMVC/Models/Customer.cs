using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mcbaMVC.Models
{
    public class Customer
    {
        /// <summary>
        /// Represents a customer who holds accounts with the bank.
        /// Includes identity and contact information.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Range(1000, 9999, ErrorMessage = "Customer ID must be exactly 4 digits long.")]
        public int CustomerID { get; set; }

        [Required, StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public required string Name { get; set; }

        [RegularExpression(@"\d{3}\s\d{3}\s\d{3}", ErrorMessage = "TFN should be entered in the format 123 456 789.")]
        public string? TFN { get; set; }

        [StringLength(50, ErrorMessage = "Address can only be up to 50 characters.")]
        public string? Address { get; set; }

        [StringLength(40, ErrorMessage = "City name cannot be longer than 40 characters.")]
        public string? City { get; set; }

        [StringLength(3, MinimumLength = 2, ErrorMessage = "State should be a valid 2 or 3 letter abbreviation.")]
        public string? State { get; set; }

        [RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must be exactly four numbers.")]
        public string? Postcode { get; set; }

        [RegularExpression(@"^04\d{2}\s\d{3}\s\d{3}$", ErrorMessage = "Provide mobile in the format 04XX XXX XXX.")]
        public string? Mobile { get; set; }

        public ICollection<Account> CustomerAccounts { get; set; } = new List<Account>();

        // 🔹 Add this → 1-to-1 relationship with Login
        public Login Login { get; set; } = null!;

    }
}
