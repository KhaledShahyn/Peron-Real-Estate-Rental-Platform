using FinalProject.src.Application.DTOs;
using FinalProject.src.Domain.Entities;

namespace FinalProject.src.Application.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetUserNotificationsAsync();
        Task<string> MarkAllAsReadAsync();
        Task<Notification> CreateNotificationAsync(string titleTemplate, string messageTemplate, string propertyTitle, string propertyLocation);
        Task DeleteNotificationAsync(int notificationId);
    }

}
