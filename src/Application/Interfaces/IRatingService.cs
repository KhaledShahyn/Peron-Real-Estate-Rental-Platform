using FinalProject.src.Application.DTOs;

namespace FinalProject.src.Application.Interfaces
{
    public interface IRatingService
    {
        Task<List<object>> AddRatingAsync(CreateRatingDto dto);
        Task<string> UpdateRatingAsync(UpdateRatingDto dto);
        Task<string> DeleteRatingAsync(int ratingId);
        Task<List<object>> GetRatingsForPropertyAsync(int propertyId);
        Task<List<PropertyResponseDTO>> GetTopRatedPropertiesAsync(int top = 5);
        Task<List<PropertyResponseDTO>> GetMostAreaAsync(int top = 5);
        Task<List<PropertyResponseDTO>> GetHighestPricedPropertiesAsync();
        Task<List<PropertyResponseDTO>> GetLowestPricedPropertiesAsync();
        Task<List<object>> GetAllRatingsAsync();
    }
}
