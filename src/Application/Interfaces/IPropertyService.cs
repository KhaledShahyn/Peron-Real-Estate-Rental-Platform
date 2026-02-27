using FinalProject.src.Application.DTOs;
using FinalProject.src.Domain.Entities;

namespace FinalProject.src.Application.Interfaces
{
    public interface IPropertyService
    {
        Task<PropertyResponseDTO> CreatePropertyAsync(PropertyDTO propertyDTO);
        Task<PropertyResponseDTO?> GetPropertyByIdAsync(int propertyId);
        Task<List<PropertyResponseDTO>> FilterPropertiesAsync(PropertyFilterDTO filter);
        Task<List<PropertyResponseDTO>> GetAllPropertiesAsync();
        Task<List<PendingPropertyDTO>> GetAllPendingPropertiesAsync();
        Task<bool> AddImagesToPropertyAsync(int propertyId, List<IFormFile> images);
        Task<bool> UpdatePropertyAsync(int propertyId, PropertyDTO propertyDTO);
        Task<bool> DeletePropertyAsync(int propertyId);
        Task<List<PropertyResponseDTO>> GetPropertiesByLocationAsync(string location);
        Task<List<PropertyResponseDTO>> GetPropertiesByPriceAsync(decimal minPrice, decimal maxPrice);
        Task<List<PropertyResponseDTO>> GetPropertiesByDateRangeAsync(DateTime from, DateTime to);
        Task<List<Property>> GetNearestPropertiesAsync(double userLat, double userLon, int maxResults);
        Task<string> CreatePendingPropertyAsync(PropertyDTO dto);
        Task<string> ConfirmPaymentAsync(string session_id);
    }
}

