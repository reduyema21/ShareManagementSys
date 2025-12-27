using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    namespace SaccoManagement.Services
    {
        public interface IDashboardService  //a service used to fetch a dashboard data
        {
            Task<DashboardViewModel> GetDashboardDataAsync();
            Task<List<MonthlyShareData>> GetShareGrowthDataAsync(int months = 12);
            Task<MemberDistributionData> GetMemberDistributionAsync();
            Task<List<MonthlyTransactionData>> GetTransactionOverviewAsync(int months = 12);
        }
    }
}