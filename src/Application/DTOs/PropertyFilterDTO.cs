namespace FinalProject.src.Application.DTOs
{
    public class PropertyFilterDTO
    {
        public string? Location { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? RentType { get; set; } // "Daily" or "Monthly"
        public int? Area { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Floor { get; set; }
        public double? MinRating { get; set; }
        public bool? IsFurnished { get; set; }
        public bool? HasBalcony { get; set; }
        public bool? HasInternet { get; set; }
        public bool? HasSecurity { get; set; }
        public bool? HasElevator { get; set; }
        public bool? AllowsPets { get; set; }
        public bool? SmokingAllowed { get; set; }
        public int? MinBookingDays { get; set; }

        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }
        //public double? Latitude { get; set; }
        //public double? Longitude { get; set; }
        //public double? MaxDistanceKm { get; set; }
    }

}
