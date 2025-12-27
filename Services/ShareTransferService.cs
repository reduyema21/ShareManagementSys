using Microsoft.EntityFrameworkCore;

using SaccoShareManagementSys.Models;

using SaccoShareManagementSys.Data;

using SaccoShareManagementSys.Services;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public class ShareTransferService : IShareTransferService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShareTransferService> _logger;

        public ShareTransferService(ApplicationDbContext context, ILogger<ShareTransferService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, ShareTransfer? Transfer)> CreateTransferAsync(ShareTransferViewModel model)
        {
            // Start database transaction for atomicity
            //using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Find both shareholders
                var fromShareholder = await _context.Shareholders
                    .FirstOrDefaultAsync(s => s.ShareholderId == model.FromShareholderId);

                var toShareholder = await _context.Shareholders
                    .FirstOrDefaultAsync(s => s.ShareholderId == model.ToShareholderId);

                // Step 2: Validate shareholders exist
                if (fromShareholder == null)
                    return (false, $"Sender shareholder not found", null);

                if (toShareholder == null)
                    return (false, $"Receiver shareholder not found", null);

                // Step 3: Validate shareholders are different
                if (model.FromShareholderId == model.ToShareholderId)
                    return (false, "Sender and receiver cannot be the same shareholder", null);

                // Step 4: Validate both shareholders are active
                if (fromShareholder.Status != "Active")
                    return (false, $"Sender shareholder is not active", null);

                if (toShareholder.Status != "Active")
                    return (false, $"Receiver shareholder is not active", null);

                // Step 5: Validate sufficient balance
                if (fromShareholder.CurrentBalance < model.ShareAmount)
                    return (false,
                        $"Insufficient balance. Available: ETB {fromShareholder.CurrentBalance:N2}, Required: ETB {model.ShareAmount:N2}",
                        null);

                // Step 6: Update balances
                fromShareholder.CurrentBalance -= model.ShareAmount;
                toShareholder.CurrentBalance += model.ShareAmount;

                // Step 7: Create ShareTransfer record
                var transfer = new ShareTransfer
                {
                    FromShareholderId = model.FromShareholderId,
                    ToShareholderId = model.ToShareholderId,
                    ShareAmount = model.ShareAmount,
                    TransferDate = DateTime.Now,
                    Status = "Completed",
                    Notes = model.Notes ?? "Share transfer",
                    CreatedAt = DateTime.Now
                };

                _context.ShareTransfers.Add(transfer);

                // Step 8: Get or create Share records for transactions
                var fromShare = await _context.Shares
                    .FirstOrDefaultAsync(s => s.ShareholderId == fromShareholder.ShareholderId);

                var toShare = await _context.Shares
                    .FirstOrDefaultAsync(s => s.ShareholderId == toShareholder.ShareholderId);

                // Step 9: Create Transaction records (Debit for sender)
                if (fromShare != null)
                {
                    var debitTransaction = new Transaction
                    {
                        ShareId = fromShare.ShareId,
                        TransactionType = "Debit",
                        Amount = model.ShareAmount,
                        TransactionDate = DateTime.Now,
                        Description = $"Transfer to {toShareholder.FullName}",
                        PaymentMethod = "Transfer",
                        Status = "Completed"
                    };
                    _context.Transactions.Add(debitTransaction);
                }

                // Step 10: Create Transaction record (Credit for receiver)
                if (toShare != null)
                {
                    var creditTransaction = new Transaction
                    {
                        ShareId = toShare.ShareId,
                        TransactionType = "Credit",
                        Amount = model.ShareAmount,
                        TransactionDate = DateTime.Now,
                        Description = $"Transfer from {fromShareholder.FullName}",
                        PaymentMethod = "Transfer",
                        Status = "Completed"
                    };
                    _context.Transactions.Add(creditTransaction);
                }

                // Step 11: Save all changes
                await _context.SaveChangesAsync();

                // Step 12: Commit transaction
                //await transaction.CommitAsync();

                _logger.LogInformation(
                    "Share transfer completed: TransferId={TransferId}, Amount={Amount}, From={FromId}, To={ToId}",
                    transfer.TransferId, model.ShareAmount, model.FromShareholderId, model.ToShareholderId);

                return (true, "Transfer completed successfully", transfer);
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                //await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating share transfer");
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateTransferAsync(ShareTransferViewModel model)
        {
            try
            {
                var transfer = await _context.ShareTransfers
                    .FirstOrDefaultAsync(t => t.TransferId == model.TransferId);

                if (transfer == null)
                {
                    return (false, "Share transfer not found.");
                }

                // Update fields
                transfer.FromShareholderId = model.FromShareholderId;
                transfer.ToShareholderId = model.ToShareholderId;
                transfer.ShareAmount = model.ShareAmount;
                transfer.TransferDate = model.TransferDate;
                transfer.Notes = model.Notes;
                transfer.Status = model.Status;

                // Optional: UpdatedAt timestamp (only if you add the column)
                //transfer.UpdatedAt = DateTime.UtcNow;

                _context.ShareTransfers.Update(transfer);
                await _context.SaveChangesAsync();

                return (true, "Share transfer updated successfully.");
            }
            catch (Exception ex)
            {
                // Log or handle
                return (false, $"Error updating transfer: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteTransferAsync(int id)
        {
            try
            {
                var transfer = await _context.ShareTransfers
                    .FirstOrDefaultAsync(t => t.TransferId == id);

                if (transfer == null)
                {
                    return (false, "Share transfer not found.");
                }

                _context.ShareTransfers.Remove(transfer);
                await _context.SaveChangesAsync();

                return (true, "Share transfer deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting transfer: {ex.Message}");
            }
        }



        public async Task<ShareTransferIndexViewModel> GetAllTransfersAsync(ShareTransferFilter? filter = null)
        {
            filter ??= new ShareTransferFilter();

            var query = _context.ShareTransfers
                .Include(st => st.FromShareholder)
                .Include(st => st.ToShareholder)
                .AsQueryable();

            // Filter by search term (search in from/to names)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(t =>
                    t.FromShareholder!.FullName.ToLower().Contains(term) ||
                    t.ToShareholder!.FullName.ToLower().Contains(term));
            }

            // Filter by date range
            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(t => t.TransferDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                // include whole day
                var to = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TransferDate <= to);
            }

            var transfers = await query
                .OrderByDescending(t => t.TransferDate)
                .Select(t => new ShareTransferListItem
                {
                    TransferId = t.TransferId,
                    FromShareholderId = t.FromShareholderId,
                    FromShareholderName = t.FromShareholder!.FullName,
                    ToShareholderId = t.ToShareholderId,
                    ToShareholderName = t.ToShareholder!.FullName,
                    ShareAmount = t.ShareAmount,
                    TransferDate = t.TransferDate,
                    Status = t.Status,
                    Notes = t.Notes
                })
                .ToListAsync();

            // compute statistics (simple example)
            var statistics = new ShareTransferStatistics
            {
                TotalTransfers = await _context.ShareTransfers.CountAsync(),

                TotalAmountTransferred = await _context.ShareTransfers
                    .SumAsync(t => (decimal?)t.ShareAmount) ?? 0,

                ActiveShareholders = await _context.Shareholders
                    .CountAsync(s => s.Status == "Active"),

                TransfersThisMonth = await _context.ShareTransfers
                    .CountAsync(t => t.TransferDate.Year == DateTime.Now.Year &&
                                     t.TransferDate.Month == DateTime.Now.Month),

                // ⭐ NEW — AVERAGE TRANSFER
                AverageTransferAmount = await _context.ShareTransfers.AnyAsync()
                    ? await _context.ShareTransfers.AverageAsync(t => t.ShareAmount)
                    : 0,


            };


            return new ShareTransferIndexViewModel
            {
                Transfers = transfers,
                Statistics = statistics,
                Filter = filter
            };
        }

        public async Task<ShareTransfer?> GetTransferByIdAsync(int transferId)
        {
            return await _context.ShareTransfers
                .Include(t => t.FromShareholder)
                .Include(t => t.ToShareholder)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);
        }

        public async Task<List<ShareTransferListItem>> GetTransfersByShareholderAsync(int shareholderId)
        {
            return await _context.ShareTransfers
                .Include(t => t.FromShareholder)
                .Include(t => t.ToShareholder)
                .Where(t => t.FromShareholderId == shareholderId || t.ToShareholderId == shareholderId)
                .OrderByDescending(t => t.TransferDate)
                .Select(t => new ShareTransferListItem
                {
                    TransferId = t.TransferId,
                    FromShareholderId = t.FromShareholderId,
                    FromShareholderName = t.FromShareholder!.FullName,
                    ToShareholderId = t.ToShareholderId,
                    ToShareholderName = t.ToShareholder!.FullName,
                    ShareAmount = t.ShareAmount,
                    TransferDate = t.TransferDate,
                    Status = t.Status,
                    Notes = t.Notes
                })
                .ToListAsync();
        }

        public async Task<ShareTransferStatistics> GetStatisticsAsync()
        {
            var totalTransfers = await _context.ShareTransfers.CountAsync();
            var totalAmount = await _context.ShareTransfers.SumAsync(t => (decimal?)t.ShareAmount) ?? 0;
            var activeShareholders = await _context.Shareholders.CountAsync(s => s.Status == "Active");

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var transfersThisMonth = await _context.ShareTransfers
                .CountAsync(t => t.TransferDate >= firstDayOfMonth);

            return new ShareTransferStatistics
            {
                TotalTransfers = totalTransfers,
                TotalAmountTransferred = totalAmount,
                ActiveShareholders = activeShareholders,
                TransfersThisMonth = transfersThisMonth
            };
        }

        public async Task<List<Shareholder>> GetActiveShareholdersAsync()
        {
            return await _context.Shareholders
                .Where(s => s.Status == "Active")
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }
        //2

        // 1. All requests where the user is involved
        public async Task<List<ShareTransfer>> GetSentRequestsAsync(int shareholderId)
        {
            return await _context.ShareTransfers
                .Where(t => t.FromShareholderId == shareholderId)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();
        }

        public async Task<List<ShareTransfer>> GetReceivedRequestsAsync(int shareholderId)
        {
            return await _context.ShareTransfers
                .Where(t => t.ToShareholderId == shareholderId)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();
        }

        public async Task<List<ShareTransfer>> GetRequestsForUserAsync(int shareholderId)
        {
            return await _context.ShareTransfers
                .Where(t => t.FromShareholderId == shareholderId || t.ToShareholderId == shareholderId)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();
        }

    }
}