namespace FinalProject.src.Application.DTOs
{
    public class BookingOverviewDto
    {
        public string UserName { get; set; }
        public string PropertyTitle { get; set; }
        public decimal Price { get; set; }
        public DateTime BookingDate { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; } 
        public string BookingStatus { get; set; }
    }

}
