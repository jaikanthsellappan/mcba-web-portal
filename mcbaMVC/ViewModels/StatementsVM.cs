namespace mcbaMVC.ViewModels
{
    public sealed class TransactionRowVM
    {
        public int TransactionID { get; init; }
        public string TransactionType { get; init; } = ""; // "D","W","T","B"
        public int AccountNumber { get; init; }
        public int? DestinationAccountNumber { get; init; }
        public decimal Amount { get; init; }
        public DateTime TransactionTimeLocal { get; init; } // UTC -> local
        public string? Comment { get; init; }
    }

    public sealed class StatementsVM
    {
        public int AccountNumber { get; init; }
        public string AccountType { get; init; } = "";
        public decimal CurrentBalance { get; init; }
        public decimal AvailableBalance { get; init; }

        public int Page { get; init; }
        public int TotalPages { get; init; }
        public IReadOnlyList<TransactionRowVM> Transactions { get; init; } = Array.Empty<TransactionRowVM>();
    }
}
