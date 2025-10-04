using System.ComponentModel.DataAnnotations;

namespace mcbaMVC.ViewModels
{
    public class DepositVM
    {
        [Required]
        [Display(Name = "Account")]
        public int AccountNumber { get; set; }

        [Required]
        [Range(0.01, 1_000_000_000, ErrorMessage = "Amount must be greater than 0.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string? Comment { get; set; }
    }
}
