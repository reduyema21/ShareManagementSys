using Microsoft.EntityFrameworkCore;
using SaccoShareManagementSys.Data;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.Services;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ApplicationDbContext context, ILogger<TransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ShareTransaction>> GetUserTransactionsAsync(int shareholderId)
        {
            return await _context.ShareTransactions
                .Include(t => t.Share)
                .Where(t => t.Share!.ShareholderId == shareholderId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }



        public async Task<TransactionIndexViewModel> GetAllTransactionsAsync(TransactionSearchFilter? filter = null)
        {
            var query = _context.ShareTransactions
                .Include(t => t.Share)
                .ThenInclude(s => s!.Shareholder)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    query = query.Where(t =>
                        t.Description!.Contains(filter.SearchTerm) ||
                        t.Share!.Shareholder!.FullName.Contains(filter.SearchTerm) ||
                        t.Share.CertificateNumber.Contains(filter.SearchTerm));
                }

                if (!string.IsNullOrWhiteSpace(filter.TransactionType))
                {
                    query = query.Where(t => t.TransactionType == filter.TransactionType);
                }

                if (filter.FromDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= filter.ToDate.Value);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionListItem
                {
                    TransactionId = t.TransactionId,
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType,
                    Description = t.Description ?? "",
                    Amount = Math.Abs(t.Amount), // UI expects positive
                    PaymentMethod = t.PaymentMethod ?? "-",
                    Status = "Completed",

                    ShareholderName = t.Share!.Shareholder!.FullName,
                    ShareholderId = t.Share.ShareholderId,
                    ShareCertificateNumber = t.Share.CertificateNumber
                })
                .ToListAsync();

            return new TransactionIndexViewModel
            {
                Transactions = transactions,
                Statistics = await GetStatisticsAsync()
            };
        }


        public async Task<ShareTransaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.ShareTransactions
                .Include(t => t.Share)
                .ThenInclude(s => s!.Shareholder)
                .FirstOrDefaultAsync(t => t.TransactionId == id);
        }


        public async Task<(bool Success, string Message)> CreateTransactionAsync(TransactionViewModel model)
        {
            try
            {
                var transaction = new ShareTransaction
                {
                    ShareId = model.ShareId,
                    TransactionType = model.TransactionType,
                    Amount = model.TransactionType == "Debit"
                        ? -model.Amount
                        : model.Amount,
                    TransactionDate = model.TransactionDate,
                    Description = model.Description,
                    PaymentMethod = model.PaymentMethod,
                    CreatedAt = DateTime.Now
                };

                _context.ShareTransactions.Add(transaction);
                await _context.SaveChangesAsync(); // all this is save the created data in to table share transactions

                return (true, "Transaction created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create failed");
                return (false, ex.Message);
            }
        }


        public async Task<(bool Success, string Message)> UpdateTransactionAsync(TransactionViewModel model)
        {
            var transaction = await _context.ShareTransactions.FindAsync(model.TransactionId);
            if (transaction == null)
                return (false, "Transaction not found");

            // Keep transaction type
            transaction.TransactionType = model.TransactionType;

            //  preserve the sign convention (Debit stored as negative)
            transaction.Amount = model.TransactionType == "Debit"
                ? -Math.Abs(model.Amount)  // ensure negative
                : Math.Abs(model.Amount);  // ensure positive

            transaction.Description = model.Description;
            transaction.PaymentMethod = model.PaymentMethod;
            transaction.Status = model.Status;

            await _context.SaveChangesAsync();
            return (true, "Transaction updated successfully");
        }

        public async Task<(bool Success, string Message)> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.ShareTransactions.FindAsync(id);
            if (transaction == null)
                return (false, "Transaction not found");

            _context.ShareTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return (true, "Transaction deleted successfully");
        }

        public async Task<List<Share>> GetSharesByShareholderIdAsync(int shareholderId)
        {
            return await _context.Shares
                .Where(s => s.ShareholderId == shareholderId)
                .ToListAsync();
        }

        //to read data from the table ShareTransaction
        public async Task<TransactionStatistics> GetStatisticsAsync()
        {
            var allTransactions = await _context.ShareTransactions.ToListAsync();

            return new TransactionStatistics
            {
                TotalTransactions = allTransactions.Count,

                TotalCredits = allTransactions
                    .Where(t => t.Amount > 0)
                    .Sum(t => t.Amount),

                TotalDebits = allTransactions
                    .Where(t => t.Amount < 0)
                    .Sum(t => Math.Abs(t.Amount)),

                NetBalance = allTransactions.Sum(t => t.Amount),

                MonthlyCredits = allTransactions
                    .Where(t => t.Amount > 0 && t.TransactionDate.Month == DateTime.Now.Month)
                    .Sum(t => t.Amount),

                MonthlyDebits = allTransactions
                    .Where(t => t.Amount < 0 && t.TransactionDate.Month == DateTime.Now.Month)
                    .Sum(t => Math.Abs(t.Amount))
            };
        }

        //2
        public async Task<ShareTransaction?> GetUserLastTransactionAsync(int shareholderId)
        {
            return await _context.ShareTransactions
                .Include(t => t.Share)
                .Where(t => t.Share!.ShareholderId == shareholderId)
                .OrderByDescending(t => t.TransactionDate)
                .FirstOrDefaultAsync();
        }


        public async Task<(decimal totalShares, decimal totalValue)> GetUserTotalSharesAsync(int shareholderId)
        {
            var shares = await _context.Shares
                .Where(s => s.ShareholderId == shareholderId)
                .ToListAsync();

            decimal totalShares = shares.Sum(s => s.NumberOfShares);
            decimal totalValue = shares.Sum(s => s.ShareValue);

            return (totalShares, totalValue);
        }
    }

}





//namespace SaccoShareManagementSys.Services
//{
//    public class TransactionService : ITransactionService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<TransactionService> _logger;

//        public TransactionService(ApplicationDbContext context, ILogger<TransactionService> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        public async Task<TransactionIndexViewModel> GetAllTransactionsAsync(TransactionSearchFilter? filter = null)
//        {
//            var query = _context.ShareTransactions
//                .Include(t => t.Share)
//                    .ThenInclude(s => s!.Shareholder)
//                .AsQueryable();

//            // Apply filters only if filter is provided
//            if (filter != null)
//            {
//                // Search term
//                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
//                {
//                    query = query.Where(t =>
//                        t.Description!.Contains(filter.SearchTerm) ||
//                        t.TransactionId.ToString().Contains(filter.SearchTerm) ||
//                        t.Share!.Shareholder!.FullName.Contains(filter.SearchTerm));
//                }

//                // Transaction type filter
//                if (!string.IsNullOrWhiteSpace(filter.TransactionType) && filter.TransactionType != "all")
//                {
//                    query = query.Where(t => t.TransactionType == filter.TransactionType);
//                }

//                // Payment method filter
//                if (!string.IsNullOrWhiteSpace(filter.PaymentMethod) && filter.PaymentMethod != "all")
//                {
//                    query = query.Where(t => t.PaymentMethod == filter.PaymentMethod);
//                }

//                // Status filter
//                if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "all")
//                {
//                    query = query.Where(t => t.Status == filter.Status);
//                }

//                // Start date filter (only apply if set)
//                if (filter.StartDate.HasValue)
//                {
//                    query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);
//                }

//                // End date filter (only apply if set)
//                if (filter.EndDate.HasValue)
//                {
//                    // Include the whole day
//                    var endDate = filter.EndDate.Value.Date.AddDays(1);
//                    query = query.Where(t => t.TransactionDate < endDate);
//                }
//            }

//            // Fetch transactions ordered by date
//            var transactions = await query
//                .OrderByDescending(t => t.TransactionDate)
//                .Select(t => new TransactionListItem
//                {
//                    TransactionId = t.TransactionId,
//                    ShareId = t.ShareId,
//                    ShareholderName = t.Share!.Shareholder!.FullName,
//                    ShareCertificateNumber = t.Share.CertificateNumber,
//                    TransactionType = t.TransactionType,
//                    Amount = t.Amount,
//                    TransactionDate = t.TransactionDate,
//                    Description = t.Description ?? "",
//                    PaymentMethod = t.PaymentMethod,
//                    Status = t.Status ?? ""
//                })
//                .ToListAsync();

//            // Fetch statistics (optional)
//            var statistics = await GetStatisticsAsync();

//            return new TransactionIndexViewModel
//            {
//                Transactions = transactions,
//                Statistics = statistics,
//                Filter = filter ?? new TransactionSearchFilter()
//            };
//        }


//        public async Task<ShareTransaction?> GetTransactionByIdAsync(int id)
//        {
//            return await _context.ShareTransactions
//                .Include(t => t.Share)
//                    .ThenInclude(s => s!.Shareholder)
//                .FirstOrDefaultAsync(t => t.TransactionId == id);
//        }

//        public async Task<(bool Success, string Message)> CreateTransactionAsync(TransactionViewModel model)
//        {
//            var strategy = _context.Database.CreateExecutionStrategy();

//            return await strategy.ExecuteAsync(async () =>
//            {
//                // Start a transaction inside the strategy
//                await using var dbTransaction = await _context.Database.BeginTransactionAsync();
//                try
//                {
//                    // Validate share exists
//                    var share = await _context.Shares
//                        .Include(s => s.Shareholder)
//                        .FirstOrDefaultAsync(s => s.ShareId == model.ShareId);

//                    if (share == null)
//                        return (false, "Share certificate not found");

//                    // Create transaction
//                    var transaction = new ShareTransaction
//                    {
//                        ShareId = model.ShareId,
//                        TransactionType = model.TransactionType,
//                        Amount = model.Amount,
//                        TransactionDate = model.TransactionDate,
//                        Description = model.Description,
//                        PaymentMethod = model.PaymentMethod,
//                        Status = model.Status,
//                        Notes = model.Notes,
//                        CreatedAt = DateTime.Now
//                    };

//                    _context.ShareTransactions.Add(transaction!);

//                    // Update shareholder balance
//                    if (model.Status == "Completed")
//                    {
//                        if (model.TransactionType == "Credit" || model.TransactionType == "Deposit")
//                        {
//                            share.Shareholder!.CurrentBalance += model.Amount;
//                            share.Shareholder.TotalShares += model.Amount;
//                        }
//                        else if (model.TransactionType == "Debit" || model.TransactionType == "Withdrawal")
//                        {
//                            if (share.Shareholder!.CurrentBalance < model.Amount)
//                                return (false, $"Insufficient balance. Available: KES {share.Shareholder.CurrentBalance:N2}");

//                            share.Shareholder.CurrentBalance -= model.Amount;
//                            share.Shareholder.TotalShares -= model.Amount;
//                        }
//                    }

//                    await _context.SaveChangesAsync();
//                    await dbTransaction.CommitAsync();

//                    _logger.LogInformation("Transaction created: {TransactionId} - {Type} - KES {Amount}",
//                        transaction.TransactionId, transaction.TransactionType, transaction.Amount);

//                    return (true, "Transaction created successfully");
//                }
//                catch (Exception ex)
//                {
//                    await dbTransaction.RollbackAsync();
//                    _logger.LogError(ex, "Error creating transaction");
//                    return (false, $"An error occurred: {ex.Message}");
//                }
//            });
//        }


//        public async Task<(bool Success, string Message)> UpdateTransactionAsync(TransactionViewModel model)
//        {
//            try
//            {
//                var transaction = await _context.ShareTransactions
//                    .FirstOrDefaultAsync(t => t.TransactionId == model.TransactionId);

//                if (transaction == null)
//                {
//                    return (false, "Transaction not found");
//                }

//                // Only allow updating certain fields (not amount or type to maintain integrity)
//                transaction.ShareId = model.ShareId;  // <-- updated ShareId
//                transaction.Amount = model.Amount;
//                transaction.TransactionType = model.TransactionType;
//                transaction.TransactionDate = model.TransactionDate;
//                transaction.Description = model.Description;
//                transaction.PaymentMethod = model.PaymentMethod;

//                transaction.Status = model.Status;
//                transaction.Notes = model.Notes;

//                await _context.SaveChangesAsync();

//                _logger.LogInformation("Transaction updated: {TransactionId}", transaction.TransactionId);

//                return (true, "Transaction updated successfully");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating transaction");
//                return (false, $"An error occurred: {ex.Message}");
//            }
//        }

//        public async Task<(bool Success, string Message)> DeleteTransactionAsync(int id)
//        {
//            var strategy = _context.Database.CreateExecutionStrategy();

//            return await strategy.ExecuteAsync(async () =>
//            {
//                await using var dbTransaction = await _context.Database.BeginTransactionAsync();

//                try
//                {
//                    var transaction = await _context.ShareTransactions
//                        .Include(t => t.Share)
//                            .ThenInclude(s => s!.Shareholder)
//                        .FirstOrDefaultAsync(t => t.TransactionId == id);

//                    if (transaction == null)
//                    {
//                        return (false, "Transaction not found");
//                    }

//                    // Reverse the balance changes if transaction was completed
//                    if (transaction.Status == "Completed" && transaction.Share != null)
//                    {
//                        if (transaction.TransactionType == "Credit" || transaction.TransactionType == "Deposit")
//                        {
//                            transaction.Share.Shareholder!.CurrentBalance -= transaction.Amount;
//                            transaction.Share.Shareholder.TotalShares -= transaction.Amount;
//                        }
//                        else if (transaction.TransactionType == "Debit" || transaction.TransactionType == "Withdrawal")
//                        {
//                            transaction.Share.Shareholder!.CurrentBalance += transaction.Amount;
//                            transaction.Share.Shareholder.TotalShares += transaction.Amount;
//                        }
//                    }

//                    _context.ShareTransactions.Remove(transaction);
//                    await _context.SaveChangesAsync();
//                    await dbTransaction.CommitAsync();

//                    _logger.LogInformation("Transaction deleted: {TransactionId}", transaction.TransactionId);

//                    return (true, "Transaction deleted successfully");
//                }
//                catch (Exception ex)
//                {
//                    await dbTransaction.RollbackAsync();
//                    _logger.LogError(ex, "Error deleting transaction");
//                    return (false, $"An error occurred: {ex.Message}");
//                }
//            });
//        }


//        public async Task<TransactionStatistics> GetStatisticsAsync()
//        {
//            var transactions = await _context.Transactions
//                .Where(t => t.Status == "Completed")
//                .ToListAsync();

//            // credit
//            var totalCredits = transactions
//                .Where(t =>
//                    t.TransactionType == "Credit" ||
//                    t.TransactionType == "Deposit" ||
//                    t.TransactionType == "Dividend" ||
//                    t.TransactionType == "Purchase"
//                )
//                .Sum(t => t.Amount);

//            // debit
//            var totalDebits = transactions
//                .Where(t =>
//                    t.TransactionType == "Debit" ||
//                    t.TransactionType == "Withdrawal" ||
//                    t.TransactionType == "Transfer"
//                )
//                .Sum(t => t.Amount);

//            // the current month only
//            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

//            var monthlyCredits = transactions
//                .Where(t =>
//                    t.TransactionDate >= firstDayOfMonth &&
//                    (
//                        t.TransactionType == "Credit" ||
//                        t.TransactionType == "Deposit" ||
//                        t.TransactionType == "Dividend" ||
//                        t.TransactionType == "Purchase"
//                    )
//                )
//                .Sum(t => t.Amount);

//            var monthlyDebits = transactions
//                .Where(t =>
//                    t.TransactionDate >= firstDayOfMonth &&
//                    (
//                        t.TransactionType == "Debit" ||
//                        t.TransactionType == "Withdrawal" ||
//                        t.TransactionType == "Transfer"
//                    )
//                )
//                .Sum(t => t.Amount);

//            return new TransactionStatistics
//            {
//                TotalTransactions = transactions.Count,
//                TotalCredits = totalCredits,
//                TotalDebits = totalDebits,
//                NetAmount = totalCredits - totalDebits,
//                MonthlyCredits = monthlyCredits,
//                MonthlyDebits = monthlyDebits,
//                TransactionsThisMonth = transactions.Count(t => t.TransactionDate >= firstDayOfMonth)
//            };
//        }
//        private async Task<List<MonthlyTransactionFlow>> GetTransactionFlowDataAsync(int months)
//        {
//            var startDate = DateTime.Now.AddMonths(-months).Date;
//            var creditTypes = new[] { "Purchase", "Dividend" };
//            var debitTypes = new[] { "Withdrawal", "Transfer" };

//            var data = await _context.Transactions
//                .Where(t => t.TransactionDate >= startDate)
//                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
//                .Select(g => new
//                {
//                    Year = g.Key.Year,
//                    Month = g.Key.Month,
//                    Credits = g.Where(t => creditTypes.Contains(t.TransactionType)).Sum(t => (decimal?)t.Amount) ?? 0,
//                    Debits = g.Where(t => debitTypes.Contains(t.TransactionType)).Sum(t => (decimal?)t.Amount) ?? 0
//                })
//                .OrderBy(x => x.Year).ThenBy(x => x.Month)
//                .ToListAsync();

//            // Fill missing months
//            var result = new List<MonthlyTransactionFlow>();
//            for (int i = months - 1; i >= 0; i--)
//            {
//                var date = DateTime.Now.AddMonths(-i);
//                var existing = data.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);

//                result.Add(new MonthlyTransactionFlow
//                {
//                    Month = date.ToString("MMM"),
//                    Credits = existing?.Credits ?? 0,
//                    Debits = existing?.Debits ?? 0
//                });
//            }

//            return result;
//        }

//        private async Task<TransactionBreakdown> GetTransactionBreakdownAsync()
//        {
//            var transactions = await _context.Transactions.ToListAsync();

//            return new TransactionBreakdown
//            {
//                PurchaseAmount = transactions.Where(t => t.TransactionType == "Purchase").Sum(t => t.Amount),
//                PurchaseCount = transactions.Count(t => t.TransactionType == "Purchase"),
//                TransferAmount = transactions.Where(t => t.TransactionType == "Transfer").Sum(t => t.Amount),
//                TransferCount = transactions.Count(t => t.TransactionType == "Transfer"),
//                DividendAmount = transactions.Where(t => t.TransactionType == "Dividend").Sum(t => t.Amount),
//                DividendCount = transactions.Count(t => t.TransactionType == "Dividend"),
//                WithdrawalAmount = transactions.Where(t => t.TransactionType == "Withdrawal").Sum(t => t.Amount),
//                WithdrawalCount = transactions.Count(t => t.TransactionType == "Withdrawal")
//            };
//        }


//        public async Task<List<Share>> GetSharesByShareholderIdAsync(int shareholderId)
//        {
//            return await _context.Shares
//                .Where(s => s.ShareholderId == shareholderId)
//                .OrderBy(s => s.CertificateNumber)
//                .ToListAsync();
//        }
//    }
//}