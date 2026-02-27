using FinalProject.src.Application.DTOs;

namespace FinalProject.src.Application.Interfaces
{
    public interface IAppRatingService
    {
        Task<bool> AddRatingAsync(CreateAppRatingDto dto, string userId);
        Task<double> GetAverageRatingAsync();
    }
}
