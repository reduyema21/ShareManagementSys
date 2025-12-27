using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public interface IShareholderService
    {
        Task<ShareholderIndexViewModel> GetAllShareholdersAsync(ShareholderSearchFilter? filter = null);
        Task<Shareholder?> GetShareholderByIdAsync(int id);
        Task<(bool Success, string Message)> CreateShareholderAsync(ShareholderViewModel model);
        Task<(bool Success, string Message)> UpdateShareholderAsync(ShareholderViewModel model);
        Task<(bool Success, string Message)> DeleteShareholderAsync(int id);
        Task<ShareholderStatistics> GetStatisticsAsync();
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
        Task<List<Shareholder>> GetActiveShareholdersAsync();
        Task<Shareholder?> GetByIdAsync(int shareholderId);
        Task<Shareholder?> GetByUserIdAsync(string userId);

    }
}
