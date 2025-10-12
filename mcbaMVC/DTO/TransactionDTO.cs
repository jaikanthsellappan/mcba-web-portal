namespace mcbaMVC.DTO
{
    public class TransactionDTO
    {
        public int TransactionID { get; set; }
        public string TransactionTimeUtc { get; set; } = string.Empty; // string from API
        public decimal Amount { get; set; }
        public string Comment { get; set; } = string.Empty; // string from API
    }
}
