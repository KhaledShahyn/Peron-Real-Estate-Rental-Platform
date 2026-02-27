using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.src.Infrastructure.Services
{
    public class RatingService : IRatingService
    {
        private readonly ApllicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IPropertyService _propertyService;

        public RatingService(ApllicationDbContext context, IAuthService authService, IPropertyService propertyService ,IConfiguration configuration)
        {
            _context = context;
            _authService = authService;
            _propertyService = propertyService;
            _configuration = configuration;
        }
        public async Task<List<object>> GetAllRatingsAsync()
        {
            var ratings = await _context.Ratings
                .Include(r => r.User)
                .Select(r => new
                {
                    r.RatingId,
                    UserName = r.User.UserName,
                    r.Stars,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            var result = ratings.Select(r => new
            {
                r.RatingId,
                r.UserName,
                r.Stars,
                r.Comment,
                r.CreatedAt,
                TimeAgo = GetTimeAgo(r.CreatedAt)
            }).ToList();

            return result.Cast<object>().ToList();
        }

        public async Task<List<object>> AddRatingAsync(CreateRatingDto dto)
        {
            var user = await _authService.ValidateUserAsync();

            if (dto.Stars == null && string.IsNullOrWhiteSpace(dto.Comment))
                throw new ArgumentException("You must provide either a star rating or a comment.");

            var propertyExists = await _context.Properties.AnyAsync(p => p.PropertyId == dto.PropertyId);
            if (!propertyExists)
                throw new ArgumentException("The specified property does not exist.");

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.PropertyId == dto.PropertyId && r.UserId == user.Id);

            if (existingRating != null)
                throw new InvalidOperationException("You have already rated this property. You can update your rating instead.");

            var rating = new Rating
            {
                PropertyId = dto.PropertyId,
                UserId = user.Id,
                Stars = dto.Stars,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            var ratings = await _context.Ratings
                .Where(r => r.PropertyId == dto.PropertyId)
                .Include(r => r.User)
                .Select(r => new
                {
                    r.RatingId,
                    UserName = r.User.UserName,
                    r.Stars,
                    r.Comment,
                    r.CreatedAt,
                    TimeAgo = GetTimeAgo(r.CreatedAt)
                })
                .ToListAsync();

            return ratings.Cast<object>().ToList();
        }





        public async Task<string> UpdateRatingAsync(UpdateRatingDto dto)
        {
            var user = await _authService.ValidateUserAsync();

            var rating = await _context.Ratings.FirstOrDefaultAsync(r => r.RatingId == dto.RatingId && r.UserId == user.Id);

            if (rating == null)
                return "BAD_REQUEST: Rating not found or you do not own it.";

            if (dto.Stars == null && string.IsNullOrWhiteSpace(dto.Comment))
                return "BAD_REQUEST: You must provide either a star rating or a comment.";

            if (dto.Stars != null)
                rating.Stars = dto.Stars;

            if (!string.IsNullOrWhiteSpace(dto.Comment))
                rating.Comment = dto.Comment;

            await _context.SaveChangesAsync();
            return "Rating updated successfully.";
        }



        public async Task<string> DeleteRatingAsync(int ratingId)
        {
            var user = await _authService.ValidateUserAsync();

            var rating = await _context.Ratings.FirstOrDefaultAsync(r => r.RatingId == ratingId && r.UserId == user.Id);
            if (rating == null)
                return "BAD_REQUEST: Rating not found or you do not own it.";

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
            return "Rating deleted successfully.";
        }



        public async Task<List<object>> GetRatingsForPropertyAsync(int propertyId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.PropertyId == propertyId)
                .Include(r => r.User)
                .Select(r => new
                {
                    r.RatingId,
                    UserName = r.User.UserName,
                    r.Stars,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            var result = ratings.Select(r => new
            {
                r.RatingId,
                r.UserName,
                r.Stars,
                r.Comment,
                r.CreatedAt,
                TimeAgo = GetTimeAgo(r.CreatedAt)
            }).ToList();

            return result.Cast<object>().ToList();
        }
        public async Task<List<PropertyResponseDTO>> GetTopRatedPropertiesAsync(int top = 5)
        {
            var topRatedProperties = await _context.Ratings
                .Where(r => r.Stars.HasValue)
                .OrderByDescending(r => r.Stars)
                .Include(r => r.Property)
                    .ThenInclude(p => p.Images)
                .Include(r => r.Property.ratings)
                .Select(r => r.Property)
                .Distinct()
                .Take(top)
                .ToListAsync();

            return topRatedProperties.Select(MapToDTO).ToList();
        }

        public async Task<List<PropertyResponseDTO>> GetMostAreaAsync(int top = 5)
        {
            var mostAreaProperties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.ratings)
                .OrderByDescending(p => p.Area)
                .Take(top)
                .ToListAsync();

            return mostAreaProperties.Select(MapToDTO).ToList();
        }
        public async Task<List<PropertyResponseDTO>> GetHighestPricedPropertiesAsync()
        {
            var properties = await _context.Properties
                .Include(p => p.Images)
                .OrderByDescending(p => p.Price)
                .Take(5)
                .ToListAsync();

            return properties.Select(MapToDTO).ToList();

        }
        public async Task<List<PropertyResponseDTO>> GetLowestPricedPropertiesAsync()
        {
            var properties = await _context.Properties
                .Include(p => p.Images)
                .OrderBy(p => p.Price)
                .Take(5)
                .ToListAsync();

            return properties.Select(MapToDTO).ToList();
        }



        private  static string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;

            if (span.TotalMinutes < 1)
                return "الآن";
            if (span.TotalMinutes < 60)
                return $"منذ {(int)span.TotalMinutes} دقيقة";
            if (span.TotalHours < 24)
                return $"منذ {(int)span.TotalHours} ساعة";
            if (span.TotalDays < 30)
                return $"منذ {(int)span.TotalDays} يوم";

            return dateTime.ToString("dd/MM/yyyy");
        }
        private PropertyResponseDTO MapToDTO(Property property)
        {
            string baseUrl = _configuration["BaseUrl"];

            return new PropertyResponseDTO
            {
                PropertyId = property.PropertyId,
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
                HasBalcony = property.HasBalcony,
                HasElevator = property.HasElevator,
                HasSecurity = property.HasSecurity,
                Floor = property.Floor,
                RentType = property.RentType,
                MinBookingDays = property.MinBookingDays,
                IsFurnished = property.IsFurnished,
                Area = property.Area,
                AverageRating = property.ratings != null && property.ratings.Any(r => r.Stars.HasValue)
                                ? property.ratings.Where(r => r.Stars.HasValue).Average(r => r.Stars.Value)
                                : 0,
                Images = property.Images?.Select(i => $"{baseUrl}{i.ImageUrl}").ToList() ?? new List<string>()
            };
        }


    }

}
