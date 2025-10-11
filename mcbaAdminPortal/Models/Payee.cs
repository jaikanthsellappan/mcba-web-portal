namespace mcbaAdminPortal.Models
{
    public class Payee
    {
        public int PayeeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Postcode { get; set; }
        public string? Phone { get; set; }
    }
}
