namespace FinalProject.src.Application.DTOs
{
    public class AdminDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalProperties { get; set; }
        public int TotalRents { get; set; }
        public string UsersGrowthPercent { get; set; }
        public string PropertiesGrowthPercent { get; set; }
        public string RentsGrowthPercent { get; set; }
    }

}
