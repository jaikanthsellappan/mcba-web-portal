namespace mcbaMVC.ViewModels
{
    public class ConfirmVM
    {
        // Common
        public TxnType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }          // <-- added
        public string? Comment { get; set; }

        // Deposit/Withdraw
        public int? AccountNumber { get; set; }
        public string? AccountTypeLabel { get; set; }
        public decimal? CurrentBalance { get; set; }
        public decimal? AvailableBalance { get; set; }

        // Transfer
        public int? FromAccountNumber { get; set; }
        public string? FromTypeLabel { get; set; }
        public decimal? FromCurrentBalance { get; set; }
        public decimal? FromAvailableBalance { get; set; }

        public int? ToAccountNumber { get; set; }
        public string? ToTypeLabel { get; set; }
        public decimal? ToCurrentBalance { get; set; }
    }
}
