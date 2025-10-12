using System.ComponentModel.DataAnnotations;

namespace mcbaMVC.ViewModels
{
    public class TransferVM
    {
        [Required]
        [Display(Name="From account")]
        public int FromAccountNumber { get; set; }

        [Required]
        [Display(Name="To account")]
        public int ToAccountNumber { get; set; }

        [Required]
        [Range(0.01, 1_000_000_000, ErrorMessage = "Amount must be greater than 0.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount can have up to 2 decimal places only.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(30, ErrorMessage = "Comment cannot exceed 30 characters.")]
        [Display(Name = "Comment (max 30 characters)")]
        public string? Comment { get; set; }
    }
}
