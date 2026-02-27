namespace FinalProject.src.Application.Interfaces
{
    public interface IBlacklistService
    {
        Task AddTokenToBlacklistAsync(string token, DateTime expiration);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}
