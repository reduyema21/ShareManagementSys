using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SaccoShareManagementSys.ViewModels
{
    public class DividendViewModel
    {
        public int DividendId { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Dividend Year *")]
        public int Year { get; set; } = DateTime.Now.Year;

        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Please select a valid month")]
        [Display(Name = "Dividend Month *")]
        public int Month { get; set; } = DateTime.Now.Month;

        [Required(ErrorMessage = "Total profit is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total profit must be greater than 0")]
        [Display(Name = "Total Profit (KES) *")]
        [DataType(DataType.Currency)]
        public decimal TotalProfit { get; set; }

        [Required(ErrorMessage = "Total shares is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total shares must be greater than 0")]
        [Display(Name = "Total Shares *")]
        public decimal TotalShares { get; set; }

        [Required(ErrorMessage = "Dividend rate is required")]
        [Range(0, 100, ErrorMessage = "Dividend rate must be between 0 and 100")]
        [Display(Name = "Dividend Rate (%) *")]
        public decimal DividendRate { get; set; }

        [Display(Name = "Total Dividend Paid (KES)")]
        [DataType(DataType.Currency)]
        public decimal TotalDividendPaid { get; set; }

        [Required]
        [Display(Name = "Distribution Date *")]
        [DataType(DataType.Date)]
        public DateTime DistributionDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Status *")]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Calculated properties
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");
        public decimal DividendPerShare => TotalShares > 0 ? TotalDividendPaid / TotalShares : 0;

        // Dropdown lists
        public SelectList? Years { get; set; }
        public SelectList? Months { get; set; }
        public SelectList? StatusList { get; set; }
        public List<ShareholderDividend>? Shareholders { get; set; }
    }

    public class DividendIndexViewModel
    {
        public List<DividendListItem> Dividends { get; set; } = new();
        public DividendStatistics Statistics { get; set; } = new();
        public DividendSearchFilter Filter { get; set; } = new();
        public DividendChartData ChartData { get; set; } = new();
        // Chart Data
        public List<MonthlyDividendTrend> DividendTrendData { get; set; } = new();
        public List<MonthlyShareFlow> ShareFlowData { get; set; } = new();
    }



    public class DividendChartData
    {
        public List<decimal>? BalanceTrend { get; set; }
        public List<decimal>? DividendTrend { get; set; }
        public List<decimal>? SharesIn { get; set; }
        public List<decimal>? SharesOut { get; set; }
        public List<string>? Labels { get; set; }
    }


    public class DividendListItem
    {
        public int DividendId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalProfit { get; set; }
        public decimal TotalShares { get; set; }
        public decimal DividendRate { get; set; }
        public decimal TotalDividendPaid { get; set; }
        public decimal DividendPerShare { get; set; }
        public DateTime DistributionDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DividendStatistics
    {
        public int TotalDividendRecords { get; set; }
        public decimal TotalDividendsPaid { get; set; }
        public decimal TotalProfitDistributed { get; set; }
        public decimal AverageDividendRate { get; set; }
        public decimal DividendsThisYear { get; set; }
    }

    public class DividendSearchFilter
    {
        public string? SearchTerm { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public string? Status { get; set; }
    }

    public class DividendCalculationViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? Status { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal DividendRate { get; set; }
        public List<ShareholderDividend> ShareholderDividends { get; set; } = new();
        public decimal TotalDividendAmount { get; set; }
        public DividendChartData? ChartData { get; set; }
        public decimal TotalSharesInvolved { get; set; }
    }

    public class ShareholderDividend
    {
        public int ShareholderId { get; set; }
        public string ShareholderName { get; set; } = string.Empty;
        public decimal TotalShares { get; set; }
        public decimal DividendAmount { get; set; }
        public decimal DividendRate { get; set; }
    }
    public class MonthlyDividendTrend
    {
        public string Month { get; set; } = string.Empty;
        public decimal DividendAmount { get; set; }
        public decimal DividendRate { get; set; }
    }

    public class MonthlyShareFlow
    {
        public string Month { get; set; } = string.Empty;
        public decimal SharesIn { get; set; }
        public decimal SharesOut { get; set; }
    }

}
