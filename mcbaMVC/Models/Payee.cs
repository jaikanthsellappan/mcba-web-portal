using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mcbaMVC.Models
{
    /// <summary>
    /// Represents a third-party payee to whom a customer can schedule bill payments.
    /// </summary>
    public class Payee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayeeID { get; set; }

        [Required, StringLength(50, ErrorMessage = "Payee name cannot exceed 50 characters.")]
        public required string Name { get; set; }

        [Required, StringLength(50, ErrorMessage = "Payee address cannot exceed 50 characters.")]
        public required string Address { get; set; }

        [Required, StringLength(40, ErrorMessage = "City name cannot exceed 40 characters.")]
        public required string City { get; set; }

        [Required, StringLength(3, MinimumLength = 2, ErrorMessage = "State should be 2–3 characters (e.g., VIC, NSW).")]
        public required string State { get; set; }

        [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must contain 4 digits only.")]
        public required string Postcode { get; set; }

        [Required, RegularExpression(@"\(0\d\)\s\d{4}\s\d{4}", ErrorMessage = "Phone must follow format (0X) XXXX XXXX.")]
        public required string Phone { get; set; }
    }
}
