using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;


namespace SaccoShareManagementSys.ViewModels
{
    public class TransactionViewModel
    {
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Please select a shareholder")]
        [Display(Name = "Shareholder *")]
        public int ShareholderId { get; set; }

        [Required(ErrorMessage = "Please select share certificate")]
        [Display(Name = "Share Certificate *")]
        public int ShareId { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [Display(Name = "Transaction Type *")]
        public string TransactionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount (KES) *")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Transaction Date *")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200)]
        [Display(Name = "Description *")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [Required]
        [Display(Name = "Status *")]
        public string Status { get; set; } = "Completed";

        [StringLength(500)]
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Additional properties for display
        public string? ShareholderName { get; set; }
        public string? ShareCertificateNumber { get; set; }

        // Dropdown lists
        public SelectList? Shareholders { get; set; }
        public SelectList? Shares { get; set; }
        public SelectList? TransactionTypes { get; set; }
        public SelectList? PaymentMethods { get; set; }
        public SelectList? StatusList { get; set; }
    }

    public class TransactionIndexViewModel
    {
        public List<TransactionListItem> Transactions { get; set; } = new();
        public TransactionStatistics Statistics { get; set; } = new();
        public TransactionSearchFilter Filter { get; set; } = new();

        // Chart Data
        public List<MonthlyTransactionFlow> TransactionFlowData { get; set; } = new();
        public TransactionBreakdown BreakdownData { get; set; } = new();
    }

    public class TransactionListItem
    {
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; } 
        public string Status { get; set; } = string.Empty;
        public string ShareholderName { get; set; } = string.Empty;
        public int ShareholderId { get; set; }
        public string ShareCertificateNumber { get; set; } = "";
    }

    public class TransactionStatistics
    {
        public int TotalTransactions { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal NetBalance { get; set; }
        public decimal MonthlyCredits { get; set; }
        public decimal MonthlyDebits { get; set; }
    }

    public class TransactionSearchFilter
    {
        public string? SearchTerm { get; set; }
        public string? TransactionType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class MonthlyTransactionFlow
    {
        public string Month { get; set; } = string.Empty;
        public decimal Credits { get; set; }
        public decimal Debits { get; set; }
    }

    public class TransactionBreakdown
    {
        public decimal PurchaseAmount { get; set; }
        public int PurchaseCount { get; set; }
        public decimal TransferAmount { get; set; }
        public int TransferCount { get; set; }
        public decimal DividendAmount { get; set; }
        public int DividendCount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public int WithdrawalCount { get; set; }
    }
}