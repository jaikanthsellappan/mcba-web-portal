using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mcbaMVC.Models
{
    /// <summary>
    /// Represents a financial account belonging to a customer.
    /// Includes balance and account type.
    /// </summary>
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [Range(1000, 9999, ErrorMessage = "Account number should be a 4-digit code.")]
        public int AccountNumber { get; set; }

        [Required, RegularExpression(@"^[CS]$", ErrorMessage = "Type must be 'C' (Checking) or 'S' (Savings).")]
        public required string AccountType { get; set; }

        [Required(ErrorMessage = "Account must be linked to a valid customer.")]
        public int CustomerID { get; set; }

        [Required, DataType(DataType.Currency, ErrorMessage = "Balance should be a valid currency amount.")]
        [Column(TypeName = "money")]
        public decimal Balance { get; set; }

        public Customer Customer { get; set; } = null!;
        public ICollection<Transaction> RelatedTransactions { get; set; } = new List<Transaction>();
        public ICollection<BillPay> RelatedBillPays { get; set; } = new List<BillPay>();
    }
}
