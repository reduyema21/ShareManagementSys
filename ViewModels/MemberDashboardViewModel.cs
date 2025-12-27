using SaccoShareManagementSys.Models;
using System;
using System.Collections.Generic;

namespace SaccoShareManagementSys.ViewModels
{
    public class MemberDashboardViewModel
    {
        // Share Summary
        public decimal TotalShares { get; set; } = 0;
        public decimal TotalShareValue { get; set; } = 0;

        // Dividends
        public decimal TotalDividends { get; set; } = 0;

        // Transactions
        public ShareTransaction? LastTransaction { get; set; }
        public List<ShareTransaction> RecentTransactions { get; set; } = new();

        // Share Transfers
        public List<ShareTransfer> SentTransfers { get; set; } = new();
        public List<ShareTransfer> ReceivedTransfers { get; set; } = new();
        public List<ShareTransfer> AllTransfers { get; set; } = new();
    }
}
