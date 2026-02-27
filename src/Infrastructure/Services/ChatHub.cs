using Microsoft.AspNetCore.SignalR;
using FinalProject.src.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FinalProject.src.Application.DTOs;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalProject.src.Infrastructure.Data;

namespace FinalProject.src.Infrastructure.Services
{
    public class ChatHub : Hub
    {
        private readonly ApllicationDbContext _context;

        public ChatHub(ApllicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> SendMessageAsync(string senderId, string receiverId, string message)
        {
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(message))
            {
                return false;
            }


            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, message);
            await Clients.Group(senderId).SendAsync("ReceiveMessage", senderId, message);

            return true;
        }
    }
}
