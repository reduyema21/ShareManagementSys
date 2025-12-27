using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public interface IShareService
    {
        Task<List<Share>> GetActiveSharesAsync();
        Task<ShareIndexViewModel> GetAllSharesAsync(ShareSearchFilter? filter = null);
        Task<Share?> GetShareByIdAsync(int id);
        Task<(bool Success, string Message)> CreateShareAsync(ShareViewModel model);
        Task<(bool Success, string Message)> UpdateShareAsync(ShareViewModel model);
        Task<(bool Success, string Message)> DeleteShareAsync(int id);
        Task<ShareStatistics> GetStatisticsAsync();
        Task<bool> CertificateNumberExistsAsync(string certificateNumber, int? excludeId = null);
        Task<string> GenerateNextCertificateNumberAsync();
        //Task<object> GetUserSharesSummaryAsync(string userId);

        Task<(decimal totalShares, decimal totalValue)> GetUserSharesSummaryAsync(int shareholderId);
        Task<List<Share>> GetSharesByShareholderIdAsync(int shareholderId);


    }
}