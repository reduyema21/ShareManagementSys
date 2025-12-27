namespace SaccoShareManagementSys.ViewModels
{
    public class DashboardViewModel
    {
        // Cards Data
        public int TotalShareholders { get; set; }
        public int ActiveShareholders { get; set; }
        public decimal TotalSharesValue { get; set; }
        public int TotalTransactions { get; set; }
        public int MonthlyTransactions { get; set; }
        public decimal TotalDividendsPaid { get; set; }
        public int TotalCertificates { get; set; }

        //top shareholder
        public List<TopShareholderItem> TopShareholders { get; set; } = new();

        // recent activity
        public List<RecentActivityItem> RecentActivities { get; set; } = new();

        // Chart Data - Share Growth Trend
        public List<MonthlyShareData> ShareGrowthData { get; set; } = new();

        // Chart Data - Member Distribution
        public MemberDistributionData MemberDistribution { get; set; } = new();

        // Chart Data - Transaction Overview
        public List<MonthlyTransactionData> TransactionOverview { get; set; } = new();

        // Chart Data - Monthly Revenue
        public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
    }

    // Supporting Classes
    public class TopShareholderItem
    {
        public int ShareholderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal TotalShares { get; set; }
        public decimal CurrentBalance { get; set; }
        public int NumberOfCertificates { get; set; }
    }

    public class RecentActivityItem
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class MonthlyShareData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalSharesIssued { get; set; }
        public decimal TotalSharesValue { get; set; }
        public int NewShareholders { get; set; }
    }

    public class MemberDistributionData
    {
        public int ActiveMembers { get; set; }
        public int InactiveMembers { get; set; }
        public int PremiumMembers { get; set; }
        public int NewMembers { get; set; }
    }

    public class MonthlyTransactionData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Credits { get; set; }
        public decimal Debits { get; set; }
    }

    public class MonthlyRevenueData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}