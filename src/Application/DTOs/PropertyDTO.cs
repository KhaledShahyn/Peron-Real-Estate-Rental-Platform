using FinalProject.src.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinalProject.src.Application.DTOs
{
    public class PropertyDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [JsonIgnore]
        public string? OwnerId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string RentType { get; set; } = "Monthly"; // "Daily" or "Monthly"
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public bool HasInternet { get; set; }
        public bool AllowsPets { get; set; }
        public int? Area { get; set; }
        public bool SmokingAllowed { get; set; }
       //public double? Rating { get; set; }
        public int? Floor { get; set; }
        public bool IsFurnished { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasElevator { get; set; }
        public int? MinBookingDays { get; set; }

        [Required]
        public DateTime AvailableFrom { get; set; }

        [Required]
        public DateTime AvailableTo { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// عند الإرسال: يتم رفع الصور كـ IFormFile
        /// </summary>
        [JsonIgnore] // تجاهل الصور عند استرجاع البيانات من الـ API
        public List<IFormFile>? Images { get; set; }

        /// <summary>
        /// عند الاسترجاع: يتم إرسال روابط الصور المخزنة
        /// </summary>
        //public List<string>? ImageUrls { get; set; } 
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        //public List<Rating>? ratings { get; set; }
    }

}
