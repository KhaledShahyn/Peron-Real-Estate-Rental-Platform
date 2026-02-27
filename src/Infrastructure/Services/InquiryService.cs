using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace FinalProject.src.Infrastructure.Services
{
    public class InquiryService : IInquiryService
    {
        private readonly ApllicationDbContext _context;
        private readonly IAuthService _authService;

        public InquiryService(ApllicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task SubmitInquiryAsync(InquiryDto dto)
        {
            var user = await _authService.ValidateUserAsync(); 
            var inquiry = new Inquiry
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Message = dto.Message,
                UserId = user?.Id 
            };

            _context.Inquiries.Add(inquiry);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Inquiry>> GetAllInquiriesAsync()
        {
            return await _context.Inquiries.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }

        public async Task ReplyToInquiryAsync(int inquiryId, string replyMessage)
        {
            var inquiry = await _context.Inquiries.FindAsync(inquiryId);
            if (inquiry != null)
            {
                inquiry.AdminReply = replyMessage;
                inquiry.RepliedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Inquiry>> GetUserInquiriesAsync()
        {
            var user = await _authService.ValidateUserAsync();

            var inquiries = await _context.Inquiries
                .Where(i => i.UserId == user.Id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return inquiries;
        }
    }

}
