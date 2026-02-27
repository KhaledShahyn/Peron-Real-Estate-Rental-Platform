using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Http;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Application.DTOs;
using FinalProject.src.Infrastructure.Data;
using FinalProject.src.Domain.Helper;

namespace FinalProject.src.Infrastructure.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicatiopnUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private readonly IEmailService _emailService;
        private readonly Dictionary<string, (string Code, DateTime Expiry)> _otpStore = new();
        private readonly IBlacklistService _blacklistService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApllicationDbContext _context;

        public AuthService(UserManager<ApplicatiopnUser> userManager, RoleManager<IdentityRole> roleManager,
            IOptions<JWT> jwt, IEmailService emailService, IBlacklistService blacklistService, IHttpContextAccessor httpContextAccessor, ApllicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
            _emailService = emailService;
            _blacklistService = blacklistService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        public async Task<ApplicatiopnUser> ValidateUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Invalid or expired token.");
            }

            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            var userData = await _userManager.Users
                .Include(u => u.Properties)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (userData == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            var token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token is missing.");
            }

            var isBlacklisted = await _context.BlacklistedTokens.AnyAsync(bt => bt.Token == token);
            if (isBlacklisted)
            {
                throw new UnauthorizedAccessException("Access denied: Token is revoked.");
            }

            return userData;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            var errors = new List<string>();
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                errors.Add("البريد الإلكتروني مسجل بالفعل!");
            if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber))
                errors.Add("رقم الهاتف مسجل بالفعل!");
            if (model.Password != model.ConfirmPassword)
                errors.Add("كلمة السر وتأكيدها غير متطابقين!");
            if (errors.Count > 0)
            {
                return new AuthModel { IsAuthenticated = false, Errors = errors };
            }
            var user = new ApplicatiopnUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                AccountCreationDate= DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e => e.Description));
                return new AuthModel { IsAuthenticated = false, Errors = errors };
            }
            var otp = new Random().Next(1000, 9999).ToString();
            user.OtpCode = otp;
            user.CodeExpiryTime = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            var emailSent = await _emailService.SendEmailAsync(model.Email, "Verification Code", $"Your OTP code is: {otp}");
            if (!emailSent)
            {
                return new AuthModel { Errors = new List<string> { "تم التسجيل، ولكن تعذر إرسال كود التحقق!" } };
            }

            return new AuthModel
            {
                IsAuthenticated = false,
                Message = "تم التسجيل بنجاح! تم إرسال كود التحقق إلى بريدك الإلكتروني.",
                PhoneNumber = user.PhoneNumber,
                Username = user.UserName,
                Email = user.Email,
                Errors = new List<string>()
            };
        }

        public async Task<AuthModel> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { Message = "Invalid email or password!" };

            if (!user.EmailConfirmed)
                return new AuthModel { Message = "Please verify your email before logging in." };
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            var jwtToken = await CreateJwtToken(user);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            var accessToken = new AccessToken
            {
                UserId = user.Id,
                Token = tokenString,
                ExpiresOn = jwtToken.ValidTo,
                CreatedOn = DateTime.UtcNow,
               
            };

            await _context.AccessTokens.AddAsync(accessToken);
            user.Status = "Active";
            await _context.SaveChangesAsync();
            await _userManager.UpdateAsync(user);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,//https 
                SameSite = SameSiteMode.None,
                Expires = model.RememberMe ? refreshToken.ExpiresOn : DateTime.UtcNow.AddDays(1)
            });
            var roles = await _userManager.GetRolesAsync(user);
            return new AuthModel
            {
                Message = null,
                PhoneNumber = user.PhoneNumber,
                IsAuthenticated = true,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                Token = tokenString,
                ExpiresOn = jwtToken.ValidTo,
                RefreshTokenExpiration = model.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(8)
            };
        }

        public async Task<string?> SendVerificationCodeAsync(EmailRequestDto email)
        {
            var user = await _userManager.FindByEmailAsync(email.Email);
            if (user == null)
                return null;

            var otp = new Random().Next(1000, 9999).ToString();
            user.OtpCode = otp;
            user.CodeExpiryTime = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            var emailSent = await _emailService.SendEmailAsync(email.Email, "Verification Code", $"Your OTP code is: {otp}");
            return emailSent ? "OTP sent and saved successfully" : null;
        }
        public async Task<AuthModel> VerifyOtpAsync(VerifyOtpModel otp)
        {
            var authModel = new AuthModel();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == otp.email);

            if (user == null)
                return new AuthModel { Message = "User not found." };
            if (user.OtpLockoutEnd != null && user.OtpLockoutEnd > DateTime.UtcNow)
                return new AuthModel { Message = "Too many incorrect attempts. Try again later." };
            if (user.OtpCode != otp.OtpCode || user.CodeExpiryTime < DateTime.UtcNow)
            {
                user.FailedOtpAttempts += 1;
                if (user.FailedOtpAttempts >= 5)
                {
                    user.OtpLockoutEnd = DateTime.UtcNow.AddMinutes(10);
                    await _userManager.UpdateAsync(user);
                    return new AuthModel { Message = "Too many incorrect attempts. Try again in 10 minutes." };
                }

                await _userManager.UpdateAsync(user);
                return new AuthModel { Message = "Invalid or expired OTP code!" };
            }
            user.PhoneNumberConfirmed = true;
            user.EmailConfirmed = true;
            user.OtpCode = null;
            user.CodeExpiryTime = null;
            user.FailedOtpAttempts = 0;
            user.OtpLockoutEnd = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                authModel.IsAuthenticated = false;
                authModel.Message = "Failed to update user information.";
                return authModel;
            }

            authModel.IsAuthenticated = true;
            authModel.Message = "Email verified successfully!";
            return authModel;
        }

        public async Task<AuthModel> VerifyOtpForResetPassAsync(VerifyOtpModel otp)
        {
            var authModel = new AuthModel();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == otp.email);
            if (user == null || user.OtpCode != otp.OtpCode || user.CodeExpiryTime < DateTime.UtcNow)
                return new AuthModel { Message = "Invalid or expired OTP code!" };

            //user.PhoneNumberConfirmed = true;
            //user.EmailConfirmed = true;
            //user.OtpCode = null;
            //user.CodeExpiryTime = null;
            //await _userManager.UpdateAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Message = "Email verified successfully!";
            return authModel;
        }



        public async Task<AuthModel?> ForgotPasswordAsync(EmailRequestDto otp)
        {
            if (string.IsNullOrEmpty(otp.Email))
            {
                return new AuthModel { Message = "الايميل مطلوب!" };
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == otp.Email);
            if (user == null)
            {
                return null;
            }

            try
            {
                var otpCode = new Random().Next(1000, 9999).ToString();
                user.OtpCode = otpCode;
                user.CodeExpiryTime = DateTime.UtcNow.AddMinutes(10);
                await _userManager.UpdateAsync(user);

                bool isSent = await _emailService.SendEmailAsync(otp.Email, "Verification Code", $"Your OTP code is: {otpCode}");
                if (!isSent)
                {
                    return null;
                }

                return new AuthModel { Message = "تم إرسال كود التحقق إلى البريد الإلكتروني." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ أثناء إرسال كود التحقق: {ex.Message}");
                return null;
            }
        }





        public async Task<AuthModel?> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                Console.WriteLine("❌ المستخدم غير موجود.");
                return new AuthModel { Message = "الايميل غير مسجل!", IsAuthenticated = false };
            }

            if (user.OtpCode != model.OtpCode || user.CodeExpiryTime < DateTime.UtcNow)
            {
                Console.WriteLine($"❌ كود التحقق غير صحيح أو منتهي الصلاحية! المُدخل: {model.OtpCode} المخزن: {user.OtpCode}");
                return new AuthModel { Message = "كود التحقق غير صحيح أو منتهي الصلاحية!", IsAuthenticated = false };
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                Console.WriteLine("❌ كلمة السر وتأكيدها غير متطابقين.");
                return new AuthModel { Message = "كلمة السر وتأكيدها غير متطابقين!", IsAuthenticated = false };
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"❌ أخطاء ResetPassword: {errors}");
                return new AuthModel { Message = "فشل إعادة تعيين كلمة المرور! " + errors, IsAuthenticated = false };
            }

            user.CodeExpiryTime = null;
            user.OtpCode = null;
            await _userManager.UpdateAsync(user);

            Console.WriteLine("✅ تم إعادة تعيين كلمة السر بنجاح!");
            return new AuthModel { Message = "تم إعادة تعيين كلمة السر بنجاح!", IsAuthenticated = true };
        }

       
        public async Task<AuthModel> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthModel { Message = "المستخدم غير موجود!" };

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return new AuthModel { Message = "لا يوجد توكن صالح!" };
            await _blacklistService.AddTokenToBlacklistAsync(token, DateTime.UtcNow.AddMinutes(30));
            user.RefreshTokens.Clear();
            user.Status = "Inactive";
            await _userManager.UpdateAsync(user);
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken");

            return new AuthModel
            {
                Message = "تم تسجيل الخروج بنجاح!",
                IsAuthenticated = false,
                Token = null,
                RefreshTokenExpiration = null
            };
        }

        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid user ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Sonething went wrong";
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicatiopnUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            foreach (var role in roles)
            {
                // هذا هو المهم: ClaimTypes.Role يُستخدم من قبل ASP.NET Core للتحقق من صلاحيات الوصول
                roleClaims.Add(new Claim(ClaimTypes.Role, role));

                // اختياري: لو عايز ترسل roles باسم آخر للـ Frontend مثلاً
                roleClaims.Add(new Claim("roles", role));
            }

            var claims = new List<Claim>
{
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("uid", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }
             .Union(userClaims)
             .Union(roleClaims);
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        //public async Task<AuthModel> RefreshTokenAsync()
        //{
        //    var authModel = new AuthModel();
        //    var token = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        authModel.Message = "Refresh token is missing";
        //        return authModel;
        //    }

        //    token = Uri.UnescapeDataString(token);

        //    var user = await _userManager.Users
        //        .Include(u => u.RefreshTokens)
        //        .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        //    if (user == null)
        //    {
        //        authModel.Message = "Invalid token";
        //        return authModel;
        //    }

        //    var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);
        //    var serverTime = DateTime.UtcNow;

        //    if (refreshToken == null || refreshToken.ExpiresOn < serverTime || refreshToken.RevokedOn != null)
        //    {
        //        authModel.Message = "Token expired or invalid";
        //        return authModel;
        //    }
        //    var oldAccessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //    if (!string.IsNullOrEmpty(oldAccessToken))
        //    {
        //        await _blacklistService.AddTokenToBlacklistAsync(oldAccessToken, refreshToken.ExpiresOn);
        //    }
        //    user.RefreshTokens.Remove(refreshToken);
        //    var newRefreshToken = GenerateRefreshToken();
        //    user.RefreshTokens.Add(newRefreshToken);
        //    await _userManager.UpdateAsync(user);
        //    await _context.SaveChangesAsync();
        //    var jwtToken = await CreateJwtToken(user);
        //    _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        SameSite = SameSiteMode.None,
        //        Expires = newRefreshToken.ExpiresOn
        //    });

        //    authModel.IsAuthenticated = true;
        //    authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        //    authModel.Email = user.Email;
        //    authModel.Username = user.UserName;
        //    authModel.Roles = (await _userManager.GetRolesAsync(user)).ToList();
        //    authModel.RefreshToken = newRefreshToken.Token;
        //    authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

        //    return authModel;
        //}
        public async Task<AuthModel> RefreshTokenAsync()
        {
            var authModel = new AuthModel();
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                authModel.Message = "Refresh token is missing";
                return authModel;
            }

            refreshToken = Uri.UnescapeDataString(refreshToken);

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .Include(u => u.AccessTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
            {
                authModel.Message = "Invalid refresh token";
                return authModel;
            }

            var oldRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken);
            var serverTime = DateTime.UtcNow;

            if (oldRefreshToken == null || oldRefreshToken.ExpiresOn < serverTime || oldRefreshToken.RevokedOn != null)
            {
                authModel.Message = "Token expired or invalid";
                return authModel;
            }
            var oldAccessToken = await _context.AccessTokens
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.CreatedOn)
                .FirstOrDefaultAsync();
            {
                await _blacklistService.AddTokenToBlacklistAsync(oldAccessToken.Token, oldAccessToken.ExpiresOn);
                _context.AccessTokens.Remove(oldAccessToken);
                await _context.SaveChangesAsync();
            }
            user.RefreshTokens.Remove(oldRefreshToken);
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);
            var jwtToken = await CreateJwtToken(user);
            var newAccessToken = new AccessToken
            {
                UserId = user.Id,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpiresOn = jwtToken.ValidTo,
                CreatedOn = DateTime.UtcNow
            };

            await _context.AccessTokens.AddAsync(newAccessToken);
            await _context.SaveChangesAsync();
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = newRefreshToken.ExpiresOn
            });
            authModel.Token = newAccessToken.Token;
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
            authModel.IsAuthenticated = true;


            return authModel;
        }
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow.AddHours(2);

            await _userManager.UpdateAsync(user);

            return true;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(30).AddHours(2), // تعويض فرق التوقيت
                CreatedOn = DateTime.UtcNow.AddHours(2) // تعويض فرق التوقيت
            };
        }

        //public async Task<AuthModel> VerifyOtpAsync(string phoneNumber, string otpCode)
        //{
        //    var authModel = new AuthModel();

        //    var user = await _userManager.Users
        //        .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

        //    if (user == null)
        //    {
        //        authModel.Message = "User not found!";
        //        return authModel;
        //    }

        //    if (user.OtpCode != otpCode || user.CodeExpiryTime < DateTime.UtcNow)
        //    {
        //        authModel.Message = "Invalid or expired OTP code!";
        //        return authModel;
        //    }
        //    user.PhoneNumberConfirmed = true;
        //    user.EmailConfirmed = true;
        //    user.OtpCode = null; 
        //    user.CodeExpiryTime = null; 
        //    await _userManager.UpdateAsync(user);

        //    authModel.IsAuthenticated = true;
        //    authModel.Message = "Phone number verified successfully!";
        //    return authModel;
        //}




    }
}
