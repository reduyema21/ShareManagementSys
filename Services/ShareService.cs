using Microsoft.EntityFrameworkCore;
using SaccoShareManagementSys.Data;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public class ShareService : IShareService
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShareService> _logger;

        public ShareService(ApplicationDbContext context, ILogger<ShareService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<ShareIndexViewModel> GetAllSharesAsync(ShareSearchFilter? filter = null)
        {
            var query = _context.Shares
                .Include(s => s.Shareholder)
                .AsQueryable();

            // Apply filters
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    query = query.Where(s =>
                        s.CertificateNumber.Contains(filter.SearchTerm) ||
                        s.Shareholder!.FullName.Contains(filter.SearchTerm) ||
                        s.ShareId.ToString().Contains(filter.SearchTerm));
                }

                if (!string.IsNullOrWhiteSpace(filter.ShareType) && filter.ShareType != "all")
                {
                    query = query.Where(s => s.ShareType == filter.ShareType);
                }

                if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "all")
                {
                    query = query.Where(s => s.Status == filter.Status);
                }

                if (filter.ShareholderId.HasValue)
                {
                    query = query.Where(s => s.ShareholderId == filter.ShareholderId.Value);
                }
            }
            var shares = await query
                .Include(s => s.Transactions)
                .OrderByDescending(s => s.PurchaseDate)
                .Select(s => new ShareListItem
                {
                    ShareId = s.ShareId,
                    ShareholderId = s.ShareholderId,
                    ShareholderName = s.Shareholder!.FullName,
                    CertificateNumber = s.CertificateNumber,
                    ShareType = s.ShareType,

                    InitialAmount = s.NumberOfShares * s.ShareValue,

                    CurrentBalance =
                        (s.NumberOfShares * s.ShareValue) +
                        s.Transactions!.Sum(t => t.Amount),

                    TransactionCount = s.Transactions!.Count,

                    GrowthPercent =
                        (s.NumberOfShares * s.ShareValue) == 0
                            ? 0
                            : (s.Transactions!.Sum(t => t.Amount)
                               / (s.NumberOfShares * s.ShareValue)) * 100,

                    PurchaseDate = s.PurchaseDate,
                    MaturityDate = s.MaturityDate,
                    Status = s.Status
                })
                .ToListAsync();






            // 2 Calculate statistics based on the loaded shares
            var statistics = new ShareStatistics
            {
                TotalCertificates = shares.Count,
                ActiveCertificates = shares.Count(s => s.Status == "Active"),
                TotalShareValue = shares.Sum(s => s.CurrentBalance),
                TotalShares = shares.Sum(s => s.InitialAmount),
                AverageShareValue = shares.Any() ? shares.Average(s => s.CurrentBalance) : 0
            };


            //return new ShareIndexViewModel
            //{
            //    Shares = shares,
            //    Statistics = statistics,
            //    Filter = filter ?? new ShareSearchFilter()
            //};
            return new ShareIndexViewModel
            {
                Shares = shares ?? new List<ShareListItem>(),
                Statistics = statistics ?? new ShareStatistics(),
                Filter = filter ?? new ShareSearchFilter()
            };

        }

        public async Task<List<Share>> GetActiveSharesAsync()
        {
            return await _context.Shares
                .Include(s => s.Shareholder)
                .Where(s => s.Status == "Active")
                .ToListAsync();
        }

        public async Task<Share?> GetShareByIdAsync(int id)
        {
            return await _context.Shares
                .Include(s => s.Shareholder)
                .Include(s => s.Transactions)
                .FirstOrDefaultAsync(s => s.ShareId == id);
        }

        public async Task<(bool Success, string Message)> CreateShareAsync(ShareViewModel model)
        {
            // Get EF Core execution strategy
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Begin transaction inside the execution strategy
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Check if certificate number already exists
                    if (await CertificateNumberExistsAsync(model.CertificateNumber))
                    {
                        return (false, "Certificate number already exists");
                    }

                    // Validate shareholder exists
                    var shareholder = await _context.Shareholders
                        .FirstOrDefaultAsync(s => s.ShareholderId == model.ShareholderId);

                    if (shareholder == null)
                    {
                        return (false, "Shareholder not found");
                    }

                    var share = new Share
                    {
                        ShareholderId = model.ShareholderId,
                        CertificateNumber = model.CertificateNumber,
                        ShareType = model.ShareType,
                        NumberOfShares = model.NumberOfShares,
                        ShareValue = model.ShareValue,
                        PurchaseDate = model.PurchaseDate,
                        Status = model.Status,
                        Notes = model.Notes,
                        MaturityDate = model.MaturityDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Shares.Add(share);
                    // Add initial transaction for this share
                    //var initialTransaction = new ShareTransaction
                    //{
                    //    Share = share,  // NOTE: EF will set ShareId after SaveChanges, so better to SaveChanges first or use Share navigation
                    //    Amount = model.NumberOfShares * model.ShareValue,
                    //    TransactionType = "Initial Deposit",
                    //    Description = "Initial share purchase",
                    //    TransactionDate = DateTime.Now
                    //};
                    //_context.ShareTransactions.Add(initialTransaction);


                    // Update shareholder statistics
                    shareholder.NumberOfCertificates++;
                    shareholder.TotalShares += model.NumberOfShares;                 // count only
                    shareholder.CurrentBalance += model.NumberOfShares * model.ShareValue; // money

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Share created: {ShareId} - {CertificateNumber}",
                        share.ShareId, share.CertificateNumber);

                    return (true, "Share certificate created successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating share");
                    return (false, $"Error: {ex.InnerException?.Message ?? ex.Message}");
                }
            });
        }

        public async Task<(bool Success, string Message)> UpdateShareAsync(ShareViewModel model)
        {
            // Get EF Core execution strategy
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var share = await _context.Shares
                        .Include(s => s.Shareholder)
                        .FirstOrDefaultAsync(s => s.ShareId == model.ShareId);

                    if (share == null)
                    {
                        return (false, "Share not found");
                    }

                    // Check if certificate number already exists (excluding current share)
                    if (await CertificateNumberExistsAsync(model.CertificateNumber, model.ShareId))
                    {
                        return (false, "Certificate number already exists");
                    }

                    // Calculate the difference in total value
                    var oldShares = share.NumberOfShares;
                    var newShares = model.NumberOfShares;
                    var oldValue = share.NumberOfShares * share.ShareValue;
                    var newValue = model.NumberOfShares * model.ShareValue;


                    share.CertificateNumber = model.CertificateNumber;
                    share.ShareType = model.ShareType;
                    share.NumberOfShares = model.NumberOfShares;
                    share.ShareValue = model.ShareValue;
                    share.PurchaseDate = model.PurchaseDate;
                    share.Status = model.Status;
                    share.Notes = model.Notes;
                    share.MaturityDate = model.MaturityDate;
                    share.UpdatedAt = DateTime.Now;

                    // Update shareholder totals
                    if (share.Shareholder != null)
                    {
                        share.Shareholder.TotalShares += (newShares - oldShares);
                        share.Shareholder.CurrentBalance += (newValue - oldValue);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Share updated: {ShareId} - {CertificateNumber}",
                        share.ShareId, share.CertificateNumber);

                    return (true, "Share certificate updated successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating share");
                    return (false, $"An error occurred: {ex.Message}");
                }
            });
        }


        public async Task<(bool Success, string Message)> DeleteShareAsync(int id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var share = await _context.Shares
                        .Include(s => s.Shareholder)
                        .Include(s => s.Transactions)
                        .FirstOrDefaultAsync(s => s.ShareId == id);

                    if (share == null)
                    {
                        return (false, "Share not found");
                    }

                    // Prevent delete if share has transactions
                    if (share.Transactions != null && share.Transactions.Any())
                    {
                        return (false, "Cannot delete share certificate with existing transactions");
                    }

                    var totalValue = share.NumberOfShares * share.ShareValue;

                    // Update shareholder statistics
                    if (share.Shareholder != null)
                    {
                        share.Shareholder.NumberOfCertificates--;
                        share.Shareholder.TotalShares -= share.NumberOfShares;
                        share.Shareholder.CurrentBalance -= totalValue;
                    }

                    _context.Shares.Remove(share);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Share deleted: {ShareId} - {CertificateNumber}",
                        share.ShareId, share.CertificateNumber);

                    return (true, "Share certificate deleted successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting share");
                    return (false, $"An error occurred: {ex.Message}");
                }
            });
        }


        public async Task<ShareStatistics> GetStatisticsAsync()
        {
            var totalCertificates = await _context.Shares.CountAsync();
            var activeCertificates = await _context.Shares.CountAsync(s => s.Status == "Active");

            var totalValue = await _context.Shares
                .Include(s => s.Transactions)
                .SumAsync(s =>
                    (decimal?)(
                        s.NumberOfShares * s.ShareValue +
                        s.Transactions!.Sum(t => t.Amount)
                    )) ?? 0;

            var totalShares = await _context.Shares
                .SumAsync(s => (decimal?)s.NumberOfShares) ?? 0;

            var avgValue = totalCertificates > 0 ? totalValue / totalCertificates : 0;

            return new ShareStatistics
            {
                TotalCertificates = totalCertificates,
                ActiveCertificates = activeCertificates,
                TotalShareValue = totalValue,
                TotalShares = totalShares,
                AverageShareValue = avgValue
            };
        }

        //public async Task<ShareStatistics> GetStatisticsAsync()
        //{
        //    try
        //    {
        //        var totalCertificates = await _context.Shares.CountAsync();
        //        var activeCertificates = await _context.Shares.CountAsync(s => s.Status == "Active");

        //        var totalValue = await _context.Shares
        //            .SumAsync(s => (decimal?)(s.NumberOfShares * s.ShareValue)) ?? 0;

        //        var totalShares = await _context.Shares
        //            .SumAsync(s => (decimal?)s.NumberOfShares) ?? 0;

        //        var avgValue = totalCertificates > 0 ? totalValue / totalCertificates : 0;

        //        return new ShareStatistics
        //        {
        //            TotalCertificates = totalCertificates,
        //            ActiveCertificates = activeCertificates,
        //            TotalShareValue = totalValue,
        //            TotalShares = totalShares,
        //            AverageShareValue = avgValue
        //        };
        //    }
        //    catch
        //    {
        //        return new ShareStatistics(); // << SAFE DEFAULT
        //    }
        //}

        public async Task<bool> CertificateNumberExistsAsync(string certificateNumber, int? excludeId = null)
        {
            var query = _context.Shares.Where(s => s.CertificateNumber == certificateNumber);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.ShareId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<string> GenerateNextCertificateNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"CERT-{year}-";

            var lastCertificate = await _context.Shares
                .Where(s => s.CertificateNumber.StartsWith(prefix))
                .OrderByDescending(s => s.CertificateNumber)
                .FirstOrDefaultAsync();

            if (lastCertificate == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = lastCertificate.CertificateNumber.Replace(prefix, "");
            if (int.TryParse(lastNumber, out int number))
            {
                return $"{prefix}{(number + 1):D3}";
            }

            return $"{prefix}001";
        }

        public async Task<(decimal totalShares, decimal totalValue)> GetMemberSharesSummaryAsync(int shareholderId)
        {
            var shares = await _context.Shares
                .Where(s => s.ShareholderId == shareholderId)
                .ToListAsync();

            if (!shares.Any())
                return (0, 0);

            decimal totalShares = shares.Sum(x => x.NumberOfShares);
            decimal totalValue = shares.Sum(x => x.ShareValue);

            return (totalShares, totalValue);
        }

        public async Task<(decimal totalShares, decimal totalValue)> GetUserSharesSummaryAsync(int shareholderId)
        {
            var shares = await _context.Shares
                .Where(s => s.ShareholderId == shareholderId)
                .ToListAsync();

            decimal totalShares = shares.Sum(s => s.NumberOfShares);
            decimal totalValue = shares.Sum(s => s.ShareValue);

            return (totalShares, totalValue);
        }

        public async Task<List<Share>> GetSharesByShareholderIdAsync(int shareholderId)
        {
            return await _context.Shares
                .Include(s => s.Transactions)
                .Where(s => s.ShareholderId == shareholderId)
                .OrderByDescending(s => s.PurchaseDate)
                .ToListAsync();
        }


    }
}