using FinalProject.src.Application.DTOs;

namespace FinalProject.src.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<List<PropertyResponseDTO>> AddToFavoritesAsync(string userId, int propertyId);
        Task<List<PropertyResponseDTO>> RemoveFromFavoritesAsync(string userId, int propertyId);
        Task<List<PropertyResponseDTO>> GetUserFavoritesAsync(string userId);
    }

}
