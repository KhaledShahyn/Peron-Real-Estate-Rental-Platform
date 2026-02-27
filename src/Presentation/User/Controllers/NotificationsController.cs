using FinalProject.src.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject.src.Presentation.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationsController(INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserNotifications()
        {
            var notifications = await _notificationService.GetUserNotificationsAsync();
            return Ok(notifications);
        }
        //[HttpPost]
        //public async Task<IActionResult> create(string message)
        //{
        //    var created = await _notificationService.CreateNotificationAsync(message);
        //    return Ok(created);
        //}
        //[HttpPost("mark-as-read/{id}")]
        //[Authorize]
        //public async Task<IActionResult> MarkAsRead(int id)
        //{
        //    await _notificationService.MarkAsReadAsync(id);
        //    return Ok();
        //}
        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _notificationService.MarkAllAsReadAsync();
            return Ok(new { message = result });
        }

        [HttpDelete("Delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);
                var notifications = await _notificationService.GetUserNotificationsAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
