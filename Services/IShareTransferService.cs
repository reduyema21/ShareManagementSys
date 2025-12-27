
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public interface IShareTransferService
    {
        Task<(bool Success, string Message, ShareTransfer? Transfer)> CreateTransferAsync(ShareTransferViewModel model);
        Task<ShareTransferIndexViewModel> GetAllTransfersAsync(ShareTransferFilter? filter = null);
        Task<ShareTransfer?> GetTransferByIdAsync(int transferId);
        Task<List<ShareTransferListItem>> GetTransfersByShareholderAsync(int shareholderId);
        Task<(bool Success, string Message)> UpdateTransferAsync(ShareTransferViewModel model);
        Task<(bool Success, string Message)> DeleteTransferAsync(int id);

        Task<ShareTransferStatistics> GetStatisticsAsync();
        Task<List<Shareholder>> GetActiveShareholdersAsync();
        //22
        Task<List<ShareTransfer>> GetRequestsForUserAsync(int shareholderId);
        Task<List<ShareTransfer>> GetSentRequestsAsync(int shareholderId);
        Task<List<ShareTransfer>> GetReceivedRequestsAsync(int shareholderId);
        //Task<(decimal totalShares, decimal totalValue)> GetUserSharesSummaryAsync(int shareholderId);
    }
}
