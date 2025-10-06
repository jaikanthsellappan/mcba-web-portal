namespace mcbaMVC.Models
{
    public enum BillPayPeriod
    {
        Once = 0,
        Monthly = 1
    }

    public enum BillPayStatus
    {
        Scheduled = 0,
        Processing = 1,
        Paid = 2,
        Failed = 3,
        Cancelled = 4
    }
}
