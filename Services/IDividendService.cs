using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public interface IDividendService
    {
        Task<DividendIndexViewModel> GetAllDividendsAsync(DividendSearchFilter? filter = null);
        Task<Dividend?> GetDividendByIdAsync(int id);
        Task<(bool Success, string Message)> CreateDividendAsync(DividendViewModel model);
        Task<(bool Success, string Message)> UpdateDividendAsync(DividendViewModel model);
        Task<(bool Success, string Message)> DeleteDividendAsync(int id);
        Task<(bool Success, string Message)> DistributeDividendAsync(int dividendId);
        Task<DividendStatistics> GetStatisticsAsync();
        Task<bool> DividendExistsForPeriodAsync(int year, int month, int? excludeId = null);
        Task<DividendCalculationViewModel> CalculateDividendAsync(int year, int month, decimal totalProfit, decimal dividendRate);
        //3333
        Task<int?> GetLatestDividendYearAsync();

        Task<decimal> GetUserTotalDividendsAsync(int shareholderId);
        //Task<decimal> GetUserTotalDividendsAsync(string userId);
        Task<List<Dividend>> GetUserDividendsAsync(int shareholderId);

    }

}