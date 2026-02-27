using FinalProject.src.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.src.Application.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
        Task<List<BookingOverviewDto>> GetAllBookingsOverviewAsync();
        Task<decimal> GetMonthlyIncomeAsync();
        Task<List<MonthlyIncomeDto>> GetLast12MonthsIncomeAsync();
        Task<List<UserPropertyPaymentDto>> GetUsersWithPropertyPaymentsAsync();
        Task<bool> UpdateUserAsync(string id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(string id);
        Task<double> GetAverageRatingAsync();
        Task<int> GetTotalRatingsAsync();
        Task<List<UserAppRatingDto>> GetUsersWhoRatedAppAsync();
        Task<List<MonthlyCustomerStatsDto>> GetMonthlyCustomerAnalysisAsync();
        Task<bool> DeleteAppRatingAsync(int ratingId);

    }

}
