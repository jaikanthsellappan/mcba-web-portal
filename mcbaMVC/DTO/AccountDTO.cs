namespace mcbaMVC.DTO
{
    public class AccountDTO
    {
        public int AccountNumber { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public List<TransactionDTO> Transactions { get; set; } = new();
    }
}
