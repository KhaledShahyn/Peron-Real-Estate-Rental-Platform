using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.src.Infrastructure.Services
{
    public class BlacklistService : IBlacklistService
    {
        private readonly ApllicationDbContext _context;

        public BlacklistService(ApllicationDbContext context)
        {
            _context = context;
        }
        public async Task AddTokenToBlacklistAsync(string token, DateTime expiration)
        {
            var blacklistedToken = new BlacklistedToken
            {
                Token = token,
                ExpirationDate = expiration
            };
            _context.BlacklistedTokens.Add(blacklistedToken);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            return await _context.BlacklistedTokens
                .AnyAsync(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow);
        }
        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = await _context.BlacklistedTokens
                .Where(t => t.ExpirationDate <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.BlacklistedTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }
    }
}
