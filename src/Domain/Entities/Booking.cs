using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Domain.Entities
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        [ForeignKey("Property")]
        public int? PropertyId { get; set; }
        public int? PendingPropertyId { get; set; }   // ✅ Rename for clarity
        [ForeignKey("PendingPropertyId")]
        public PendingProperty? PendingProperty { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }

        public ApplicatiopnUser? User { get; set; }
        public Payment? Payment { get; set; }
        public Property? Property { get; set; }
    }
}
