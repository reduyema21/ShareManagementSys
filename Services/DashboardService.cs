using Microsoft.EntityFrameworkCore;
using SaccoShareManagementSys.Data;
using SaccoShareManagementSys.Services.SaccoManagement.Services;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            try
            {
                var viewModel = new DashboardViewModel
                {
                    // Summary Cards
                    TotalShareholders = await _context.Shareholders.CountAsync(),
                    ActiveShareholders = await _context.Shareholders
                        .CountAsync(s => s.Status == "Active"),
                    TotalSharesValue = await _context.Shares
                        .SumAsync(s => (decimal?)(s.NumberOfShares * s.ShareValue)) ?? 0,
                    TotalTransactions = await _context.Transactions.CountAsync(),
                    MonthlyTransactions = await _context.Transactions
                        .CountAsync(t => t.TransactionDate.Month == DateTime.Now.Month &&
                                        t.TransactionDate.Year == DateTime.Now.Year),
                    TotalDividendsPaid = await _context.Dividends
                        .SumAsync(d => (decimal?)d.TotalDividendPaid) ?? 0,
                    TotalCertificates = await _context.Shares.CountAsync(),

                    // Top Shareholders
                    TopShareholders = await GetTopShareholdersAsync(),

                    // Recent Activities
                    RecentActivities = await GetRecentActivitiesAsync(),

                    // Chart Data
                    ShareGrowthData = await GetShareGrowthDataAsync(12),
                    MemberDistribution = await GetMemberDistributionAsync(),
                    TransactionOverview = await GetTransactionOverviewAsync(12),
                    MonthlyRevenue = await GetMonthlyRevenueAsync(12)
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard data");
                throw;
            }
        }

        private async Task<List<TopShareholderItem>> GetTopShareholdersAsync()
        {
            return await _context.Shareholders
                .OrderByDescending(s => s.TotalShares)
                .Take(5)
                .Select(s => new TopShareholderItem
                {
                    ShareholderId = s.ShareholderId,
                    FullName = s.FullName,
                    TotalShares = s.TotalShares,
                    CurrentBalance = s.CurrentBalance,
                    NumberOfCertificates = s.NumberOfCertificates
                })
                .ToListAsync();
        }

        private async Task<List<RecentActivityItem>> GetRecentActivitiesAsync()
        {
            var activities = new List<RecentActivityItem>();

            // Get recent shareholder additions
            var recentShareholders = await _context.Shareholders
                .OrderByDescending(s => s.CreatedAt)
                .Take(3)
                .Select(s => new RecentActivityItem
                {
                    Type = "Shareholder",
                    Title = "New Shareholder",
                    Description = $"{s.FullName} registered",
                    Timestamp = s.CreatedAt
                })
                .ToListAsync();
            activities.AddRange(recentShareholders);

            // Get recent transactions
            var recentTransactions = await _context.Transactions
                .Include(t => t.Share)
                .ThenInclude(s => s!.Shareholder)
                .Where(t => t.Share != null && t.Share.Shareholder != null)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new RecentActivityItem
                {
                    Type = "Transaction",
                    Title = t.TransactionType,
                    Description = $"ETB {t.Amount:N0} - {t.Share!.Shareholder!.FullName}",
                    Timestamp = t.CreatedAt
                })
                .ToListAsync();
            activities.AddRange(recentTransactions);

            // Sort all activities by timestamp and take top 10
            return activities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
        }

        public async Task<List<MonthlyShareData>> GetShareGrowthDataAsync(int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);

            var monthlyData = await _context.Shares
                .Where(s => s.PurchaseDate >= startDate)
                .GroupBy(s => new { s.PurchaseDate.Year, s.PurchaseDate.Month })
                .Select(g => new MonthlyShareData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    TotalSharesIssued = g.Sum(s => s.NumberOfShares),
                    TotalSharesValue = g.Sum(s => s.NumberOfShares * s.ShareValue),
                    NewShareholders = g.Select(s => s.ShareholderId).Distinct().Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToListAsync();

            // Fill in missing months with zero data
            var result = new List<MonthlyShareData>();
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var existing = monthlyData.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);

                if (existing != null)
                {
                    result.Add(existing);
                }
                else
                {
                    result.Add(new MonthlyShareData
                    {
                        Year = date.Year,
                        Month = date.Month,
                        MonthName = date.ToString("MMM yyyy"),
                        TotalSharesIssued = 0,
                        TotalSharesValue = 0,
                        NewShareholders = 0
                    });
                }
            }

            return result;
        }

        public async Task<MemberDistributionData> GetMemberDistributionAsync()
        {
            var activeCount = await _context.Shareholders.CountAsync(s => s.Status == "Active");
            var inactiveCount = await _context.Shareholders.CountAsync(s => s.Status == "Inactive");
            var premiumCount = await _context.Shareholders.CountAsync(s => s.MemberType == "Premium");
            var newCount = await _context.Shareholders
                .CountAsync(s => s.CreatedAt >= DateTime.Now.AddMonths(-1));

            return new MemberDistributionData
            {
                ActiveMembers = activeCount,
                InactiveMembers = inactiveCount,
                PremiumMembers = premiumCount,
                NewMembers = newCount
            };
        }

        public async Task<List<MonthlyTransactionData>> GetTransactionOverviewAsync(int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);

            var monthlyData = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate)
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Credits = g.Where(t => t.TransactionType == "Purchase" || t.TransactionType == "Dividend")
                              .Sum(t => (decimal?)t.Amount) ?? 0,
                    Debits = g.Where(t => t.TransactionType == "Withdrawal" || t.TransactionType == "Transfer")
                             .Sum(t => (decimal?)t.Amount) ?? 0
                })
                .ToListAsync();

            var result = new List<MonthlyTransactionData>();
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var existing = monthlyData.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);

                result.Add(new MonthlyTransactionData
                {
                    Month = date.ToString("MMM"),
                    Credits = existing?.Credits ?? 0,
                    Debits = existing?.Debits ?? 0
                });
            }

            return result;
        }

        private async Task<List<MonthlyRevenueData>> GetMonthlyRevenueAsync(int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);

            var monthlyData = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.Status == "Completed")
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(t => (decimal?)t.Amount) ?? 0
                })
                .ToListAsync();

            var result = new List<MonthlyRevenueData>();
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var existing = monthlyData.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);

                result.Add(new MonthlyRevenueData
                {
                    Month = date.ToString("MMM"),
                    Revenue = existing?.Revenue ?? 0
                });
            }

            return result;
        }
    }
}