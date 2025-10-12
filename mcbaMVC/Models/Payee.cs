using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mcbaMVC.Models
{
    public class Payee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayeeID { get; set; }

        [Required, StringLength(50, ErrorMessage = "Payee name cannot exceed 50 characters.")]
        public required string Name { get; set; }

        [Required, StringLength(50, ErrorMessage = "Address cannot exceed 50 characters.")]
        public required string Address { get; set; }

        [Required, StringLength(40, ErrorMessage = "City name cannot exceed 40 characters.")]
        public required string City { get; set; }

        [Required, StringLength(3, MinimumLength = 2, ErrorMessage = "State should be 2–3 letters (e.g., VIC, NSW).")]
        public required string State { get; set; }

        // ✅ Must be exactly 4 digits (Australian postcodes)
        [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must contain exactly 4 digits.")]
        public required string Postcode { get; set; }

        // ✅ Simplified and realistic: allows (03) 9123 4567 OR 03 9123 4567
        [Required, RegularExpression(@"^(?:\(0\d\)\s?|\d{2}\s?)\d{4}\s?\d{4}$",
            ErrorMessage = "Phone must follow format 03 9123 4567 or (03) 9123 4567.")]
        public required string Phone { get; set; }
    }
}
