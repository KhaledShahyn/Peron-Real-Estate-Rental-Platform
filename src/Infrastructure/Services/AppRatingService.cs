using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.src.Infrastructure.Services
{
    public class AppRatingService : IAppRatingService
    {
        private readonly ApllicationDbContext _context;

        public AppRatingService(ApllicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRatingAsync(CreateAppRatingDto dto, string userId)
        {

            var existing = await _context.AppRatings
                 .FirstOrDefaultAsync(r => r.UserId == userId);


            if (existing != null)
            {
                existing.Stars = dto.Stars;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.AppRatings.Add(new AppRating
                {
                    Stars = dto.Stars,
                    UserId = userId
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<double> GetAverageRatingAsync()
        {
            return await _context.AppRatings.AnyAsync()
                ? await _context.AppRatings.AverageAsync(r => r.Stars)
            : 0;
        }
    }
}
