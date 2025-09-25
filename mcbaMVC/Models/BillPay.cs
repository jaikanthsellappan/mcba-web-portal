using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mcbaMVC.Models
{
    /// <summary>
    /// Represents a scheduled payment from a customer account to a registered payee.
    /// </summary>
    public class BillPay
    {
        [Key]
        public int BillPayID { get; set; }

        [Required(ErrorMessage = "Bill must be tied to an account.")]
        public int AccountNumber { get; set; }

        [Required(ErrorMessage = "Payee ID is required for a bill payment.")]
        public int PayeeID { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Bill amount must be positive.")]
        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Schedule time must be provided.")]
        public DateTime ScheduleTimeUtc { get; set; }

        [Required]
        [RegularExpression(@"^[OM]$", ErrorMessage = "Period must be 'O' (One-off) or 'M' (Monthly).")]
        [StringLength(1)]
        public string Period { get; set; } = string.Empty;

        [ForeignKey(nameof(AccountNumber))]
        public Account Account { get; set; } = null!;
        public Payee Payee { get; set; } = null!;
    }
}
