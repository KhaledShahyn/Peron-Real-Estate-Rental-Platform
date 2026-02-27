using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.src.Domain.Entities
{
    public class Property
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropertyId { get; set; }

        [Required]
        [ForeignKey("Owner")]
        public string OwnerId { get; set; } = null!;
        public ApplicatiopnUser Owner { get; set; } = null!;

        [Required]
        public string Title { get; set; } = string.Empty;
        [Range(10, 1000, ErrorMessage = "المساحة يجب أن تكون بين 10 و 1000 متر مربع.")]
        public int? Area { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        [Required]
        public string RentType { get; set; } = "Monthly"; // "Daily" or "Monthly"

        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Floor { get; set; }

        //public double? Rating { get; set; }

        public bool IsFurnished { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasInternet { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasElevator { get; set; }
        public bool AllowsPets { get; set; }
        public bool SmokingAllowed { get; set; }

        [Required]
        public DateTime AvailableFrom { get; set; }

        [Required]
        public DateTime AvailableTo { get; set; }

        public int? MinBookingDays { get; set; }

        public string? Description { get; set; }
        public List<PropertyImage>? Images { get; set; } = new List<PropertyImage>();
        public List<Booking>? Bookings { get; set; }
        public List<Rating>?ratings { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

}
