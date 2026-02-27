using FinalProject.src.Application.Interfaces;
using FinalProject.src.Infrastructure.Data;

namespace FinalProject.src.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly ApllicationDbContext _context;

        public TokenService(ApllicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            //return await _context.RefreshToken
            //    .AnyAsync(t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
            return bool.TryParse(token, out _);
        }
    }

}
