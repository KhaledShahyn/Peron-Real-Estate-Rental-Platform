using FinalProject.src.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalProject.src.Application.DTOs;
using FinalProject.src.Infrastructure.Services;
using FinalProject.src.Infrastructure.Data;
using FinalProject.src.Application.Interfaces;

namespace FinalProject.src.Presentation.User.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApllicationDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IAuthService _authService;

        public ChatController(ApllicationDbContext context, IHubContext<ChatHub> chatHub,IAuthService authService)
        {
            _context = context;
            _chatHub = chatHub; 
            _authService = authService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto chatDto)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId))
                return Unauthorized(new { error = "المستخدم غير مصادق عليه." });

            if (string.IsNullOrEmpty(chatDto.Message) || string.IsNullOrEmpty(chatDto.ReceiverId))
            {
                return BadRequest(new { error = "بيانات غير مكتملة!" });
            }
            var receiverExists = await _context.Users.AnyAsync(u => u.Id == chatDto.ReceiverId);
            if (!receiverExists)
            {
                return BadRequest(new { error = "المستخدم المستقبل غير موجود." });
            }

            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = chatDto.ReceiverId,
                Message = chatDto.Message,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            try
            {
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();
                await _chatHub.Clients.Group(chatDto.ReceiverId).SendAsync("ReceiveMessage", senderId, chatDto.Message);
                await _chatHub.Clients.Group(senderId).SendAsync("ReceiveMessage", senderId, chatDto.Message);

                return Ok(new { message = "تم إرسال الرسالة!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ أثناء إرسال الرسالة.", details = ex.Message });
            }
        }

        [HttpGet("conversation/{userId2}")]
        public async Task<IActionResult> GetConversation(string userId2)
        {
            var userId1 = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId1))
                return Unauthorized(new { error = "المستخدم غير مصادق عليه." });
            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderBy(m => m.Timestamp)
                .Select(m => new {
                    m.Id,
                    m.Message,
                    m.Timestamp,
                    m.IsRead,
                    IsMe = m.SenderId == userId1,
                    SenderName = m.Sender.FullName,
                    SenderPhoto = m.Sender.ProfilePictureUrl
                })
                .ToListAsync();


            return Ok(messages);
        }
        [HttpGet("chats")]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "المستخدم غير مصادق عليه." });

            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToListAsync();

            var chats = messages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g =>
                {
                    var lastMessage = g.OrderByDescending(m => m.Timestamp).First();
                    var chatWithId = lastMessage.SenderId == userId ? lastMessage.ReceiverId : lastMessage.SenderId;
                    var name = lastMessage.SenderId == userId ? lastMessage.Receiver.FullName : lastMessage.Sender.FullName;
                    var photo = lastMessage.SenderId == userId ? lastMessage.Receiver.ProfilePictureUrl : lastMessage.Sender.ProfilePictureUrl;
                    var unreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead);

                    return new
                    {
                        ChatWithId = chatWithId,
                        Name = name,
                        Photo = photo,
                        LastMessage = lastMessage.Message,
                        Timestamp = GetRelativeTime(lastMessage.Timestamp),
                        IsRead = lastMessage.IsRead,
                        UnreadCount = unreadCount
                    };
                })
                .OrderByDescending(c => c.Timestamp)
                .ToList();

            return Ok(chats);
        }


        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkMessagesAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "المستخدم غير مصادق عليه." });

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            if (!unreadMessages.Any())
                return Ok(new { message = "لا توجد رسائل غير مقروءة." });

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث جميع الرسائل إلى مقروءة." });
        }

        private string GetRelativeTime(DateTime timestamp)
        {
            var span = DateTime.UtcNow - timestamp;

            if (span.TotalSeconds < 60)
                return "الآن";
            if (span.TotalMinutes < 2)
                return "منذ دقيقة";
            if (span.TotalMinutes < 60)
                return $"منذ {(int)span.TotalMinutes} دقيقة";
            if (span.TotalHours < 2)
                return "منذ ساعة";
            if (span.TotalHours < 24)
                return $"منذ {(int)span.TotalHours} ساعة";
            if (span.TotalDays < 2)
                return "منذ يوم";
            return $"منذ {(int)span.TotalDays} يوم";
        }

    }
}
