using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace FinalProject.src.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApllicationDbContext _context;
        private readonly IAuthService _authService;

        public NotificationService(ApllicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Notification> CreateNotificationAsync(string titleTemplate, string messageTemplate, string propertyTitle, string propertyLocation)
        {
            var user = await _authService.ValidateUserAsync();

            var title = string.Format(titleTemplate, propertyTitle);
            var message = string.Format(messageTemplate, propertyLocation ?? "مكان غير محدد");

            var notification = new Notification
            {
                UserId = user.Id,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }


        public async Task<List<NotificationDto>> GetUserNotificationsAsync()
        {
            var user = await _authService.ValidateUserAsync();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                TimeAgo = GetTimeAgo(n.CreatedAt)
            }).ToList();
        }
        private string GetTimeAgo(DateTime dateTime)
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




        public async Task<string> MarkAllAsReadAsync()
        {
            var user = await _authService.ValidateUserAsync();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return "All notifications are already marked as read.";

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return $"{unreadNotifications.Count} notifications marked as read.";
        }



        public async Task DeleteNotificationAsync(int notificationId)
        {
            var user = await _authService.ValidateUserAsync();
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == user.Id);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("الإشعار غير موجود أو لا يخص هذا المستخدم.");
            }
        }

    }
}
