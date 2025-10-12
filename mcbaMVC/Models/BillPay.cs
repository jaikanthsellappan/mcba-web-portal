using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mcbaMVC.Models
{
    /// Represents a scheduled payment from a customer account to a registered payee.
    public class BillPay
    {
        [Key] public int BillPayID { get; set; }

        [Required(ErrorMessage = "Bill must be tied to an account.")]
        public int AccountNumber { get; set; }

        [Required(ErrorMessage = "Payee ID is required for a bill payment.")]
        public int PayeeID { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Bill amount must be positive.")]
        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        // Store UTC; UI displays local
        [Required(ErrorMessage = "Schedule time must be provided.")]
        public DateTime ScheduleTimeUtc { get; set; }

        // Your existing period format: 'O' (once) or 'M' (monthly)
        [Required]
        [RegularExpression(@"^[OM]$", ErrorMessage = "Period must be 'O' (One-off) or 'M' (Monthly).")]
        [StringLength(1)]
        public string Period { get; set; } = "O";

        // NEW: lifecycle status (Scheduled/Processing/Paid/Failed/Cancelled)
        // Using 1-letter codes to match your current pattern.
        [Required]
        [RegularExpression(@"^[SPFCDB]$", ErrorMessage = "Status must be S, P, F, C, B or D.")]
        [StringLength(1)]
        public string Status { get; set; } = "S"; // S=Scheduled, P=Processing, F=Failed, C=Cancelled, B=Blocked, D=Paid (Done)

        // NEW: diagnostics for UI
        public DateTime? LastAttemptUtc { get; set; }

        [StringLength(300)]
        public string? LastError { get; set; }

        [ForeignKey(nameof(AccountNumber))]
        public Account? Account { get; set; }
        public Payee?  Payee   { get; set; }
    }
}
