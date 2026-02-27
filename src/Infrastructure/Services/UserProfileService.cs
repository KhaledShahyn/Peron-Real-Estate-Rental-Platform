using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GenerativeAI;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Application.DTOs;
using FinalProject.src.Infrastructure.Data;
using FinalProject.src.Domain.Entities;

public class UserProfileService
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicatiopnUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApllicationDbContext _context;
    private readonly IAuthService _authService;

    public UserProfileService(UserManager<ApplicatiopnUser> userManager, IHttpContextAccessor httpContextAccessor, ITokenService tokenService,ApllicationDbContext context, IAuthService authService)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _tokenService = tokenService;
        _context = context;
        _authService = authService;
    }
    public async Task<bool> CreateProfileAsync(UpdateProfileDto profileDto)
    {
        var user = await _authService.ValidateUserAsync();

        user.FullName = profileDto.FullName;

        if (profileDto.ProfilePicture != null && profileDto.ProfilePicture.Length > 0)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile_pictures");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(profileDto.ProfilePicture.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileDto.ProfilePicture.CopyToAsync(stream);
            }
            var baseUrl = "https://sakaniapi1.runasp.net";
            user.ProfilePictureUrl = $"{baseUrl}/profile_pictures/{fileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }



    public async Task<bool> UpdateProfileAsync(UpdateProfileDto profileDto)
    {
        var user = await _authService.ValidateUserAsync();
        if (user == null) throw new Exception("User not found");

        if (!string.IsNullOrEmpty(profileDto.FullName))
            user.FullName = profileDto.FullName;
        if (!string.IsNullOrEmpty(profileDto.PhoneNumber))
            user.PhoneNumber = profileDto.PhoneNumber;
        if (profileDto.ProfilePicture != null && profileDto.ProfilePicture.Length > 0)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile_pictures");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(profileDto.ProfilePicture.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileDto.ProfilePicture.CopyToAsync(stream);
            }
            var baseUrl = "https://sakaniapi1.runasp.net";
            user.ProfilePictureUrl = $"{baseUrl}/profile_pictures/{fileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<UserProfileDTO> GetProfileAsync()
    {
        var userData = await _authService.ValidateUserAsync();

        var roles = await _userManager.GetRolesAsync(userData);
        var role = roles.FirstOrDefault();

        return new UserProfileDTO
        {
            FullName = userData.FullName,
            Email = userData.Email,
            ProfilePictureUrl = userData.ProfilePictureUrl,
            PhoneNumber = userData.PhoneNumber,
            Role = role
            //Rating = averageRating ?? 0
        };
    }


    public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePassword)
    {
        var userData = await _authService.ValidateUserAsync();
        if (userData == null)
        {
            throw new Exception("المستخدم غير موجود.");
        }

        var passwordHasher = new PasswordHasher<ApplicatiopnUser>();
        var result = passwordHasher.VerifyHashedPassword(userData, userData.PasswordHash, changePassword.OldPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new Exception("كلمة المرور القديمة غير صحيحة.");
        }
        if (changePassword.NewPassword != changePassword.ConfirmPassword)
        {
            throw new Exception("كلمة المرور الجديدة وتأكيدها غير متطابقين.");
        }
        userData.PasswordHash = passwordHasher.HashPassword(userData, changePassword.NewPassword);
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<GetPropertyDto> GetPropertyAsync()
    {

        var userData = await _authService.ValidateUserAsync();
        return new GetPropertyDto
        {
            OwnedProperties = userData.Properties?.Select(p => new PropertyResponseDTO
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Location = p.Location,
                Price = p.Price,
                ratings = p.ratings
            }).ToList()
        };
    }
    public async Task<bool> DeleteAccount()
    {
        try
        {
            var userData = await _authService.ValidateUserAsync();
            if (userData == null)
                throw new Exception("المستخدم غير موجود.");
            var userProperties = await _context.Properties
                .Where(p => p.OwnerId == userData.Id)
                .ToListAsync();
            var propertyIds = userProperties.Select(p => p.PropertyId).ToList();
            var propertyImages = await _context.PropertiesImage
                .Where(img => propertyIds.Contains(img.PropertyId))
                .ToListAsync();
            _context.PropertiesImage.RemoveRange(propertyImages);
            var favorites = await _context.Favorites
                .Where(f => propertyIds.Contains(f.PropertyId))
                .ToListAsync();
            _context.Favorites.RemoveRange(favorites);
            var bookings = await _context.Bookings
                .Where(b => propertyIds.Contains(b.BookingId))
                .ToListAsync();
            _context.Bookings.RemoveRange(bookings);
            _context.Properties.RemoveRange(userProperties);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userData.Id)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);
            var ratings = await _context.AppRatings
                .Where(r => r.UserId == userData.Id)
                .ToListAsync();
            _context.AppRatings.RemoveRange(ratings);
            var tokens = await _context.AccessTokens
                .Where(t => t.UserId == userData.Id)
                .ToListAsync();
            _context.AccessTokens.RemoveRange(tokens);
            _context.Users.Remove(userData);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            throw new Exception($"Database error: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"General error: {ex.Message}");
        }
    }














}
