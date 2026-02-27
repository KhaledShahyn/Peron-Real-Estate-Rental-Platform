namespace FinalProject.src.Application.DTOs
{
    public class MonthlyCustomerStatsDto
    {
        public string Month { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int Bookings { get; set; }
    }

}
