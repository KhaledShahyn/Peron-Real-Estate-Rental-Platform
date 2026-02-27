using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FinalProject.src.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicatiopnUser> _userManager;
        private readonly ApllicationDbContext _context;

        public AdminService(UserManager<ApplicatiopnUser> userManager, ApllicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);
            var endOfLastMonth = startOfThisMonth.AddDays(-1);

            var usersThisMonth = await Task.FromResult(_userManager.Users.Count(u => u.AccountCreationDate >= startOfThisMonth));
            var usersLastMonth = await Task.FromResult(_userManager.Users.Count(u => u.AccountCreationDate >= startOfLastMonth && u.AccountCreationDate < startOfThisMonth));

            var propertiesThisMonth = await _context.Properties.CountAsync(p => p.CreatedAt >= startOfThisMonth);
            var propertiesLastMonth = await _context.Properties.CountAsync(p => p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfThisMonth);

            var rentsThisMonth = await _context.Bookings.CountAsync(b => b.StartDate >= startOfThisMonth);
            var rentsLastMonth = await _context.Bookings.CountAsync(b => b.StartDate >= startOfLastMonth && b.StartDate < startOfThisMonth);

            string FormatPercentage(int current, int previous)
            {
                if (previous == 0)
                {
                    if (current == 0) return "0%";
                    return "+100%";
                }

                double percent = ((double)(current - previous) / previous) * 100;
                return percent >= 0 ? $"+{percent:F2}%" : $"{percent:F2}%";
            }

            return new AdminDashboardStatsDto
            {
                TotalUsers = await Task.FromResult(_userManager.Users.Count()),
                TotalProperties = await _context.Properties.CountAsync(),
                TotalRents = await _context.Bookings.CountAsync(),

                UsersGrowthPercent = FormatPercentage(usersThisMonth, usersLastMonth),
                PropertiesGrowthPercent = FormatPercentage(propertiesThisMonth, propertiesLastMonth),
                RentsGrowthPercent = FormatPercentage(rentsThisMonth, rentsLastMonth)
            };
        }



        public async Task<List<BookingOverviewDto>> GetAllBookingsOverviewAsync()
        {
            var bookings = await _context.Bookings?
                .Include(b => b.User)
                .Include(b => b.Property)
                .ToListAsync();

            var result = bookings.Select(b => new BookingOverviewDto
            {
                UserName = b.User?.UserName ?? "Unknown",
                PropertyTitle = b.Property?.Title ?? "Unknown Property",
                Price = b.Property?.Price ?? 0,
                BookingDate = b.StartDate,
                PaymentStatus = b.Status == "Accepted" ? "Accepted" : "Unaccepted",
                PaymentMethod = "Visa",
                BookingStatus = b.Status
            }).ToList();

            return result;
        }
        public async Task<decimal> GetMonthlyIncomeAsync()
        {
            try
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); 
                Console.WriteLine($"StartOfMonth: {startOfMonth}, EndOfMonth: {endOfMonth}");
                var bookings = await _context.Bookings
                    .Where(b => b.StartDate.Date >= startOfMonth.Date && b.StartDate.Date <= endOfMonth.Date)
                    .ToListAsync();
                if (bookings.Count == 0)
                {
                    Console.WriteLine("لا توجد حجوزات في هذا الشهر.");
                }
                var monthlyIncome = bookings.Sum(b => b.TotalPrice);

                return monthlyIncome;
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء حساب الدخل الشهري", ex);
            }
        }
        public async Task<List<MonthlyIncomeDto>> GetLast12MonthsIncomeAsync()
        {
            var result = new List<MonthlyIncomeDto>();
            for (int i = 0; i < 12; i++)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var bookings = await _context.Bookings
                    .Where(b =>
                        b.StartDate.Date <= endOfMonth.Date &&
                        b.EndDate.Date >= startOfMonth.Date &&
                        b.Status == "Accepted")
                    .ToListAsync();

                var totalIncome = bookings.Sum(b => b.TotalPrice);

                result.Add(new MonthlyIncomeDto
                {
                    Month = startOfMonth.ToString("MMMM"),
                    Year = startOfMonth.Year,
                    Income = totalIncome
                });
            }
            result.Reverse();

            return result;
        }
        public async Task<List<UserPropertyPaymentDto>> GetUsersWithPropertyPaymentsAsync()
        {
            var result = await _context.Users
                .Select(u => new UserPropertyPaymentDto
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RentalCount = u.Bookings.Select(b => b.PropertyId).Distinct().Count(),
                    Date = u.Bookings.Any()
                        ? u.Bookings.Max(b => b.StartDate)
                        : default(DateTime),

                    //Status = u.Bookings.Any()? 
                    //(u.Bookings.OrderByDescending(x => x.StartDate).FirstOrDefault()
                    //.Status == "Accepted" ? "Active": "Inactive"): "Inactive"
                    Status=u.Status
                })
                .ToListAsync();

            return result;
        }



        public async Task<bool> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Status)) 
                user.Status = dto.Status;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return false;
                var userProperties = await _context.Properties
                    .Where(p => p.OwnerId == id)
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
                    .Where(n => n.UserId == id)
                    .ToListAsync();
                _context.Notifications.RemoveRange(notifications);
                var ratings = await _context.AppRatings
                    .Where(r => r.UserId == id)
                    .ToListAsync();
                _context.AppRatings.RemoveRange(ratings);
                var tokens = await _context.AccessTokens
                    .Where(t => t.UserId == id)
                    .ToListAsync();
                _context.AccessTokens.RemoveRange(tokens);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<double> GetAverageRatingAsync()
        {
            return await _context.AppRatings.AnyAsync()
                ? await _context.AppRatings.AverageAsync(r => r.Stars)
            : 0;
        }
        public async Task<int> GetTotalRatingsAsync()
        {
            return await _context.AppRatings.CountAsync();
        }
        public async Task<List<UserAppRatingDto>> GetUsersWhoRatedAppAsync()
        {
            var ratings = await _context.Ratings
                .Include(r => r.User)
                .Where(r => r.Stars > 0 && r.User != null)
                .ToListAsync();

            return ratings.Select(r => new UserAppRatingDto
            {
                Id = r.RatingId,
                Name = r.User.FullName,
                PhoneNumber = r.User.PhoneNumber,
                Email = r.User.Email,
                ProfilePictureUrl = r.User.ProfilePictureUrl,
                Rating = r.Stars,
                JoinedAt = r.User.AccountCreationDate
            }).ToList();
        }
        public async Task<bool> DeleteAppRatingAsync(int ratingId)
        {
            var rating = await _context.AppRatings.FindAsync(ratingId);

            if (rating == null)
                return false;

            _context.AppRatings.Remove(rating);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MonthlyCustomerStatsDto>> GetMonthlyCustomerAnalysisAsync()
        {
            var result = new List<MonthlyCustomerStatsDto>();

            for (int i = -5; i <= 0; i++)
            {
                var monthDate = DateTime.UtcNow.AddMonths(i);
                var startOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var monthName = startOfMonth.ToString("MMM");
                int newCustomers = await _context.Users
                    .CountAsync(u => u.AccountCreationDate >= startOfMonth && u.AccountCreationDate <= endOfMonth);

                int activeCustomers = await _context.Users
                    .CountAsync(u => u.AccountCreationDate >= startOfMonth && u.AccountCreationDate <= endOfMonth);

                int bookings = await _context.Bookings
                    .CountAsync(b => b.StartDate >= startOfMonth && b.StartDate <= endOfMonth);

                result.Add(new MonthlyCustomerStatsDto
                {
                    Month = monthName,
                    ActiveCustomers = activeCustomers,
                    NewCustomers = newCustomers,
                    Bookings = bookings
                });
            }

            return result;
        }



    }

}
