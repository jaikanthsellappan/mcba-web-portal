namespace mcbaMVC.DTO
{
    public class CustomerDTO
    {
        public int CustomerID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public LoginDTO Login { get; set; } = null!;
        public List<AccountDTO> Accounts { get; set; } = new();
    }
}
