using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using GenerativeAI;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.src.Infrastructure.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApllicationDbContext _context;
        private readonly IConfiguration _configuration;

        public FavoriteService(ApllicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<List<PropertyResponseDTO>> AddToFavoritesAsync(string userId, int propertyId)
        {
            var propertyExists = await _context.Properties.AnyAsync(p => p.PropertyId == propertyId);
            if (!propertyExists)
            {
                return null; 
            }

            var exists = await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);

            if (!exists)
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    PropertyId = propertyId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return await GetUserFavoritesAsync(userId);
        }
        public async Task<List<PropertyResponseDTO>> RemoveFromFavoritesAsync(string userId, int propertyId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return await GetUserFavoritesAsync(userId);
        }


        public async Task<List<PropertyResponseDTO>> GetUserFavoritesAsync(string userId)
        {
            string baseUrl = _configuration["BaseUrl"];

            var favorites = await _context.Favorites
                 .Include(f => f.Property)
                     .ThenInclude(p => p.Images)
                 .Include(f => f.Property)
                     .ThenInclude(p => p.ratings)
                 .Where(f => f.UserId == userId)
                 .ToListAsync();


            return favorites
                .Where(f => f.Property != null).Select(f => new PropertyResponseDTO
            {
                PropertyId = f.PropertyId,
                Title = f.Property.Title,
                Location = f.Property.Location,
                Price = f.Property.Price,
                RentType = f.Property.RentType,
                Bedrooms = f.Property.Bedrooms,
                Bathrooms = f.Property.Bathrooms,
                    AverageRating = f.Property.ratings != null && f.Property.ratings.Any(r => r.Stars.HasValue)
                    ? f.Property.ratings.Where(r => r.Stars.HasValue).Average(r => r.Stars.Value) : 0,
                    IsFurnished = f.Property.IsFurnished,
                HasBalcony = f.Property.HasBalcony,
                HasInternet = f.Property.HasInternet,
                HasSecurity = f.Property.HasSecurity,
                HasElevator = f.Property.HasElevator,
                AllowsPets = f.Property.AllowsPets,
                SmokingAllowed = f.Property.SmokingAllowed,
                AvailableFrom = f.Property.AvailableFrom,
                AvailableTo = f.Property.AvailableTo,
                Description = f.Property.Description,
                Images = f.Property.Images != null
                    ? f.Property.Images.Select(i => $"{baseUrl}{i.ImageUrl}").ToList()
                    : new List<string>()
            }).ToList();
        }


    }
}
