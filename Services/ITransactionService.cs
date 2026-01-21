using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public interface ITransactionService
    {
        Task<TransactionIndexViewModel> GetAllTransactionsAsync(TransactionSearchFilter? filter = null);

        Task<ShareTransaction?> GetTransactionByIdAsync(int id);

        Task<(bool Success, string Message)> CreateTransactionAsync(TransactionViewModel model);

        Task<(bool Success, string Message)> UpdateTransactionAsync(TransactionViewModel model);

        Task<(bool Success, string Message)> DeleteTransactionAsync(int id);

        Task<TransactionStatistics> GetStatisticsAsync();

        Task<List<Share>> GetSharesByShareholderIdAsync(int shareholderId);

        Task<IEnumerable<ShareTransaction>> GetUserTransactionsAsync(int shareholderId);
        Task<ShareTransaction?> GetUserLastTransactionAsync(int shareholderId);

    }
}
