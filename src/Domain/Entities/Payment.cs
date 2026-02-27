using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Domain.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public Booking Booking { get; set; }
    }
}
