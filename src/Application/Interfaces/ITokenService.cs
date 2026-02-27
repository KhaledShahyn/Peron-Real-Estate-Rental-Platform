namespace FinalProject.src.Application.Interfaces
{
    public interface ITokenService
    {
        Task<bool> IsTokenValidAsync(string token);
    }
}
