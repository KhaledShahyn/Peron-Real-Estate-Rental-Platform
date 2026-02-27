using FinalProject.src.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Application.DTOs
{
    public class PropertyResponseDTO
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string RentType { get; set; } = "Monthly"; // "Daily" or "Monthly"
        [Range(10, 1000, ErrorMessage = "المساحة يجب أن تكون بين 10 و 1000 متر مربع.")]
        public int? Area { get; set; }
        public string? OwnerId { get; set; }
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
        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }

        public int? MinBookingDays { get; set; }
        public double AverageRating { get; set; }
        public string? Description { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public List<Rating>? ratings { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Distance { get; set; }

        public static PropertyResponseDTO FromProperty(Property property)
        {
            return new PropertyResponseDTO
            {
                PropertyId = property.PropertyId,
                OwnerId = property.OwnerId,
                Title = property.Title,
                Location = property.Location,
                Price = property.Price,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                HasInternet = property.HasInternet,
                AllowsPets = property.AllowsPets,
                SmokingAllowed = property.SmokingAllowed,
                AvailableFrom = property.AvailableFrom,
                AvailableTo = property.AvailableTo,
                Description = property.Description,
                Images = property.Images.Select(img => img.ImageUrl).ToList(),
                HasSecurity = property.HasSecurity,
                HasElevator = property.HasElevator,
                HasBalcony = property.HasBalcony,
                Floor = property.Floor,
                RentType = property.RentType,
                //Rating = property.Rating,
                MinBookingDays = property.MinBookingDays,
                IsFurnished = property.IsFurnished,
                Area = property.Area,
                Longitude = property.Longitude,
                Latitude = property.Latitude,
                ratings=property.ratings

            };
        }
    }

}
