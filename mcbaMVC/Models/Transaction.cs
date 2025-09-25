using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mcbaMVC.Models
{
    /// <summary>
    /// Records an individual account transaction such as deposits, withdrawals, or transfers.
    /// </summary>
    public class Transaction
    {
        [Key]
        [Required]
        public int TransactionID { get; set; }

        [Required, RegularExpression(@"^[DWTBS]$", ErrorMessage = "Transaction type must be one of D, W, T, S, or B.")]
        public string TransactionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Source account number is mandatory.")]
        public int AccountNumber { get; set; }

        public int? DestinationAccountNumber { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount should be greater than zero.")]
        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [StringLength(30, ErrorMessage = "Comment must not exceed 30 characters.")]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "Transaction time is required.")]
        public DateTime TransactionTimeUtc { get; set; }

        [ForeignKey(nameof(AccountNumber))]
        public Account Account { get; set; } = null!;
    }
}
