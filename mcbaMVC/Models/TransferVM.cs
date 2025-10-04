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
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string? Comment { get; set; }
    }
}
