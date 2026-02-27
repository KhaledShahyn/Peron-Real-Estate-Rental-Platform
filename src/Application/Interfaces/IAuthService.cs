using FinalProject.src.Application.DTOs;
using FinalProject.src.Domain.Entities;

namespace FinalProject.src.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<ApplicatiopnUser> ValidateUserAsync();
        Task<AuthModel> LoginAsync(LoginModel model);
        Task<string> SendVerificationCodeAsync(EmailRequestDto mail);
        Task<AuthModel> VerifyOtpAsync(VerifyOtpModel otp);
        Task<AuthModel> ForgotPasswordAsync(EmailRequestDto otp);
        Task<AuthModel> ResetPasswordAsync(ResetPasswordModel model);
        Task<string> AddRoleAsync(AddRoleModel model);
        Task<AuthModel> RefreshTokenAsync();
        Task<bool> RevokeTokenAsync(string token);
        Task<AuthModel> LogoutAsync(string userId);
        Task<AuthModel> VerifyOtpForResetPassAsync(VerifyOtpModel otp);

    }
}
