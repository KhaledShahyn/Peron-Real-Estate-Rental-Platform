using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.src.Presentation.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminService _userService;

        public AdminDashboardController(IAdminService userService)
        {
            _userService = userService;
        }

        [HttpGet("total-Apartment-User-Rent")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _userService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        [HttpGet("latest-listings")]
        public async Task<IActionResult> GetAllRentBookings()
        {
            try
            {
                var bookings = await _userService.GetAllBookingsOverviewAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("customer-analysis")]
        public async Task<IActionResult> GetCustomerAnalysis()
        {
            var data = await _userService.GetMonthlyCustomerAnalysisAsync();
            return Ok(data);
        }

        [HttpGet("income-analysis")]
        public async Task<IActionResult> GetIncomeAnalysis()
        {
            try
            {
                var monthlyIncome = await _userService.GetMonthlyIncomeAsync();

                if (monthlyIncome < 0)
                {
                    return NotFound("لا توجد حجوزات لهذا الشهر.");
                }
                return Ok(monthlyIncome);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء جلب الدخل الشهري: {ex.Message}");
            }
        }
        [HttpGet("Get-Last-12-Months-Income")]
        public async Task<IActionResult> Getlast12MIncomeAnalysis()
        {
            try
            {
                var incomeData = await _userService.GetLast12MonthsIncomeAsync();
                return Ok(incomeData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء تحليل الدخل الشهري: {ex.Message}");
            }
        }

        [HttpGet("users-with-property-payments")]
        public async Task<IActionResult> GetUsersWithPropertyPayments()
        {
            try
            {
                var usersWithRentals = await _userService.GetUsersWithPropertyPaymentsAsync();
                return Ok(usersWithRentals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var result = await _userService.UpdateUserAsync(id, dto);
            if (!result) return NotFound(new { message = "User not found" });

            return Ok(new { message = "User updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deleted" });
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverage()
        {
            var avg = await _userService.GetAverageRatingAsync();
            return Ok(new { averageRating = avg });
        }
        [HttpGet("total-ratings")]
        public async Task<IActionResult> GetTotalRatings()
        {
            var totalRatings = await _userService.GetTotalRatingsAsync();
            return Ok(totalRatings);
        }
        [HttpGet("app-rating-users")]
        public async Task<IActionResult> GetUsersWhoRatedApp()
        {
            var result = await _userService.GetUsersWhoRatedAppAsync();
            return Ok(result);
        }
        [HttpDelete("delete-rating/{id}")]
        public async Task<IActionResult> DeleteRating(int id)
        {
            var success = await _userService.DeleteAppRatingAsync(id);
            if (!success)
                return NotFound("Rating not found.");
            return Ok(new { message = "Rating deleted successfully." });
        }



    }
}
