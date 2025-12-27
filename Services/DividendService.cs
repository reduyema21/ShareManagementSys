using Microsoft.EntityFrameworkCore;
using SaccoShareManagementSys.Data;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public class DividendService : IDividendService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DividendService> _logger;

        public DividendService(ApplicationDbContext context, ILogger<DividendService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<int?> GetLatestDividendYearAsync()
        {
            return await _context.Dividends.MaxAsync(d => (int?)d.Year);
        }

        public async Task<DividendIndexViewModel> GetAllDividendsAsync(DividendSearchFilter? filter = null)
        {
            if (filter == null)
            {
                filter = new DividendSearchFilter
                {
                    Year = DateTime.Now.Year,
                    Status = "all"
                };
            }

            if (!filter.Year.HasValue)
            {
                var latestYear = await GetLatestDividendYearAsync();
                filter.Year = latestYear ?? DateTime.Now.Year;
            }

            filter.Status ??= "all";

            var query = _context.Dividends.AsQueryable();

            if (filter.Year.HasValue)
                query = query.Where(d => d.Year == filter.Year.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "all")
                query = query.Where(d => d.Status == filter.Status);

            var dividends = await query
                .OrderByDescending(d => d.Year)
                .ThenByDescending(d => d.Month)
                .Select(d => new DividendListItem
                {
                    DividendId = d.DividendId,
                    Year = d.Year,
                    Month = d.Month,
                    MonthName = new DateTime(d.Year, d.Month, 1).ToString("MMMM"),
                    TotalProfit = d.TotalProfit,
                    TotalShares = d.TotalShares,
                    DividendRate = d.DividendRate,
                    TotalDividendPaid = d.TotalDividendPaid,
                    DividendPerShare = d.TotalShares > 0 ? d.TotalDividendPaid / d.TotalShares : 0,
                    DistributionDate = d.DistributionDate,
                    Status = d.Status
                })
                .ToListAsync();

            var statistics = await GetStatisticsAsync() ?? new DividendStatistics();

            // FIX: Wrap chart logic so failure does not break the dividend list
            List<MonthlyDividendTrend> trendData;
            List<MonthlyShareFlow> shareFlowData;

            try
            {
                trendData = await GetDividendTrendDataAsync(filter.Year.Value);
                shareFlowData = await GetShareFlowDataAsync(filter.Year.Value);
                //new List<MonthlyDividendTrend>(); new List<MonthlyShareFlow>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trend or ShareFlow failed. Charts disabled.");
                trendData = new List<MonthlyDividendTrend>();
                shareFlowData = new List<MonthlyShareFlow>();
            }

            // IMPORTANT: Removed month parsing — it causes errors in non-English locales

            return new DividendIndexViewModel
            {
                Dividends = dividends,
                Statistics = statistics,
                Filter = filter,
                DividendTrendData = trendData,
                ShareFlowData = shareFlowData
            };
        }

        // =======================
        // GET BY ID
        // =======================
        public async Task<Dividend?> GetDividendByIdAsync(int id)
            => await _context.Dividends.FirstOrDefaultAsync(d => d.DividendId == id);

        // =======================
        // CREATE
        // =======================
        public async Task<(bool Success, string Message)> CreateDividendAsync(DividendViewModel model)
        {
            try
            {
                if (await DividendExistsForPeriodAsync(model.Year, model.Month))
                    return (false, $"Dividend for {model.MonthName} {model.Year} already exists");

                var totalDividendPaid = model.TotalProfit * (model.DividendRate / 100);

                var dividend = new Dividend
                {
                    Year = model.Year,
                    Month = model.Month,
                    TotalProfit = model.TotalProfit,
                    TotalShares = model.TotalShares,
                    DividendRate = model.DividendRate,
                    TotalDividendPaid = totalDividendPaid,
                    DistributionDate = model.DistributionDate,
                    Status = model.Status,
                    Notes = model.Notes,
                    CreatedAt = DateTime.Now
                };

                _context.Dividends.Add(dividend);
                await _context.SaveChangesAsync();
                // debug - count how many records exist for the created year
                var countForYear = await _context.Dividends.CountAsync(d => d.Year == dividend.Year);
                _logger.LogInformation("Dividend created - DB count for year {Year} = {Count}", dividend.Year, countForYear);

                return (true, "Dividend record created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dividend");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        // =======================
        // UPDATE
        // =======================
        public async Task<(bool Success, string Message)> UpdateDividendAsync(DividendViewModel model)
        {
            try
            {
                var dividend = await _context.Dividends.FirstOrDefaultAsync(d => d.DividendId == model.DividendId);
                if (dividend == null) return (false, "Dividend not found");

                if (await DividendExistsForPeriodAsync(model.Year, model.Month, model.DividendId))
                    return (false, $"Dividend for {model.MonthName} {model.Year} already exists");

                var totalDividendPaid = model.TotalProfit * (model.DividendRate / 100);

                dividend.Year = model.Year;
                dividend.Month = model.Month;
                dividend.TotalProfit = model.TotalProfit;
                dividend.TotalShares = model.TotalShares;
                dividend.DividendRate = model.DividendRate;
                dividend.TotalDividendPaid = totalDividendPaid;
                dividend.DistributionDate = model.DistributionDate;
                dividend.Status = model.Status;
                dividend.Notes = model.Notes;
                dividend.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return (true, "Dividend record updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dividend");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        // =======================
        // DELETE
        // =======================
        public async Task<(bool Success, string Message)> DeleteDividendAsync(int id)
        {
            try
            {
                var dividend = await _context.Dividends.FirstOrDefaultAsync(d => d.DividendId == id);
                if (dividend == null) return (false, "Dividend not found");
                if (dividend.Status == "Distributed") return (false, "Cannot delete distributed dividend");

                _context.Dividends.Remove(dividend);
                await _context.SaveChangesAsync();
                return (true, "Dividend deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dividend");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        // =======================
        // DISTRIBUTE DIVIDEND
        // =======================
        public async Task<(bool Success, string Message)> DistributeDividendAsync(int dividendId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dividend = await _context.Dividends.FirstOrDefaultAsync(d => d.DividendId == dividendId);
                if (dividend == null) return (false, "Dividend not found");
                if (dividend.Status == "Distributed") return (false, "Dividend has already been distributed");

                var shareholders = await _context.Shareholders
                    .Where(s => s.Status == "Active" && s.TotalShares > 0)
                    .ToListAsync();

                if (!shareholders.Any()) return (false, "No active shareholders found");

                var totalShares = shareholders.Sum(s => s.TotalShares);
                var dividendPerShare = dividend.TotalDividendPaid / totalShares;

                foreach (var shareholder in shareholders)
                {
                    var shareholderDividend = shareholder.TotalShares * dividendPerShare;

                    var share = await _context.Shares
                        .FirstOrDefaultAsync(s => s.ShareholderId == shareholder.ShareholderId);

                    if (share != null)
                    {
                        var tx = new Transaction
                        {
                            ShareId = share.ShareId,
                            TransactionType = "Dividend",
                            Amount = shareholderDividend,
                            TransactionDate = dividend.DistributionDate,
                            Description = $"Dividend for {new DateTime(dividend.Year, dividend.Month, 1):MMMM yyyy}",
                            PaymentMethod = "Dividend",
                            Status = "Completed",
                            CreatedAt = DateTime.Now
                        };

                        _context.Transactions.Add(tx);
                        shareholder.CurrentBalance += shareholderDividend;
                    }
                }

                dividend.Status = "Distributed";
                dividend.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Dividend distributed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error distributing dividend");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        // =======================
        // STATISTICS
        // =======================
        public async Task<DividendStatistics> GetStatisticsAsync()
        {
            var totalRecords = await _context.Dividends.CountAsync();
            var totalPaid = await _context.Dividends.SumAsync(d => (decimal?)d.TotalDividendPaid) ?? 0;
            var totalProfit = await _context.Dividends.SumAsync(d => (decimal?)d.TotalProfit) ?? 0;
            var avgRate = totalRecords > 0
                ? await _context.Dividends.AverageAsync(d => (decimal?)d.DividendRate) ?? 0
                : 0;

            var currentYear = DateTime.Now.Year;
            var dividendsThisYear = await _context.Dividends
                .Where(d => d.Year == currentYear)
                .SumAsync(d => (decimal?)d.TotalDividendPaid) ?? 0;

            return new DividendStatistics
            {
                TotalDividendRecords = totalRecords,
                TotalDividendsPaid = totalPaid,
                TotalProfitDistributed = totalProfit,
                AverageDividendRate = avgRate,
                DividendsThisYear = dividendsThisYear
            };
        }

        // =======================
        // CHECK EXISTING PERIOD
        // =======================
        public async Task<bool> DividendExistsForPeriodAsync(int year, int month, int? excludeId = null)
        {
            var query = _context.Dividends.Where(d => d.Year == year && d.Month == month);
            if (excludeId.HasValue) query = query.Where(d => d.DividendId != excludeId.Value);
            return await query.AnyAsync();
        }

        // =======================
        // CALCULATE DIVIDEND
        // =======================
        public async Task<DividendCalculationViewModel> CalculateDividendAsync(
            int year, int month, decimal totalProfit, decimal dividendRate)
        {
            var shareholders = await _context.Shareholders
                .Where(s => s.Status == "Active" && s.TotalShares > 0)
                .ToListAsync();

            var totalShares = shareholders.Sum(s => s.TotalShares);
            var totalDividendAmount = totalProfit * (dividendRate / 100);
            var dividendPerShare = totalShares > 0 ? totalDividendAmount / totalShares : 0;

            var shareholderDividends = shareholders
                .Select(s => new ShareholderDividend
                {
                    ShareholderId = s.ShareholderId,
                    ShareholderName = s.FullName,
                    TotalShares = s.TotalShares,
                    DividendAmount = s.TotalShares * dividendPerShare,
                    DividendRate = dividendRate
                })
                .OrderByDescending(sd => sd.DividendAmount)
                .ToList();

            return new DividendCalculationViewModel
            {
                Year = year,
                Month = month,
                TotalProfit = totalProfit,
                DividendRate = dividendRate,
                ShareholderDividends = shareholderDividends,
                TotalDividendAmount = totalDividendAmount,
                TotalSharesInvolved = totalShares
            };
        }

        // =======================
        // TREND CHART DATA
        // =======================
        private async Task<List<MonthlyDividendTrend>> GetDividendTrendDataAsync(int year)
        {
            var data = await _context.Dividends
                .Where(d => d.Year == year)
                .OrderBy(d => d.Month)
                .Select(d => new MonthlyDividendTrend
                {
                    Month = new DateTime(d.Year, d.Month, 1).ToString("MMM"),
                    DividendAmount = d.TotalDividendPaid,
                    DividendRate = d.DividendRate
                })
                .ToListAsync();

            var result = new List<MonthlyDividendTrend>();

            for (int m = 1; m <= 12; m++)
            {
                var name = new DateTime(year, m, 1).ToString("MMM");
                var existing = data.FirstOrDefault(d => d.Month == name);

                result.Add(existing ?? new MonthlyDividendTrend
                {
                    Month = name,
                    DividendAmount = 0,
                    DividendRate = 0
                });
            }

            return result;
        }


        // =======================
        // SHARE FLOW CHART DATA
        // =======================
        private async Task<List<MonthlyShareFlow>> GetShareFlowDataAsync(int year)
        {
            var sharesIn = await _context.Shares
                .Where(s => s.PurchaseDate.Year == year)
                .GroupBy(s => s.PurchaseDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Amount = g.Sum(s => s.NumberOfShares)
                })
                .ToListAsync();

            var sharesOut = await _context.ShareTransfers
                .Where(t => t.TransferDate.Year == year)
                .GroupBy(t => t.TransferDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Amount = g.Sum(t => t.ShareAmount)
                })
                .ToListAsync();

            var result = new List<MonthlyShareFlow>();

            for (int m = 1; m <= 12; m++)
            {
                var monthName = new DateTime(year, m, 1).ToString("MMM");

                var inData = sharesIn.FirstOrDefault(x => x.Month == m);
                var outData = sharesOut.FirstOrDefault(x => x.Month == m);

                result.Add(new MonthlyShareFlow
                {
                    Month = monthName,
                    SharesIn = (inData?.Amount ?? 0) / 1000,
                    SharesOut = (outData?.Amount ?? 0) / 1000
                });
            }

            return result;
        }

        public async Task<decimal> GetUserTotalDividendsAsync(int shareholderId)
        {
            // Since Dividend has no ShareholderId, return total dividends for all
            return await _context.Dividends.SumAsync(d => (decimal?)d.TotalDividendPaid) ?? 0m;
        }

        public async Task<List<Dividend>> GetUserDividendsAsync(int shareholderId)
        {
            return await _context.Dividends
                .Include(d => d.Shareholder)
                .Where(d => d.ShareholderId == shareholderId)
                .OrderByDescending(d => d.Year)
                .ToListAsync();
        }



    }
}