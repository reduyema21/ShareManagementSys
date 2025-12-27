using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SaccoShareManagementSys.ViewModels
{
    public class ShareTransferViewModel
    {
        [Required(ErrorMessage = "Please select sender")]
        [Display(Name = "From Shareholder *")]
        public int FromShareholderId { get; set; }

        [Required(ErrorMessage = "Please select receiver")]
        [Display(Name = "To Shareholder *")]
        public int ToShareholderId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Share Amount (KES) *")]
        [DataType(DataType.Currency)]
        public decimal ShareAmount { get; set; }
        public int TransferId { get; set; }
        public string Status { get; set; } = "Completed";

        public SelectList? StatusList { get; set; }

        [Required]
        [Display(Name = "Transfer Date")]
        [DataType(DataType.DateTime)]
        public DateTime TransferDate { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        [MaxLength(500)]
        [Display(Name = "Transfer Notes (Optional)")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Additional properties for display
        public string? FromShareholderName { get; set; }
        public decimal? FromShareholderBalance { get; set; }
        public string? ToShareholderName { get; set; }
        public decimal? ToShareholderBalance { get; set; }

        // Dropdown lists
        public SelectList? Shareholders { get; set; }
    }

    public class ShareTransferIndexViewModel
    {
        public List<ShareTransferListItem> Transfers { get; set; } = new();
        public ShareTransferFilter Filter { get; set; } = new();
        public ShareTransferStatistics Statistics { get; set; } = new();
    }

    public class ShareTransferListItem
    {
        public int TransferId { get; set; }
        public int FromShareholderId { get; set; }
        public string FromShareholderName { get; set; } = string.Empty;
        public int ToShareholderId { get; set; }
        public string ToShareholderName { get; set; } = string.Empty;
        public decimal ShareAmount { get; set; }
        public DateTime TransferDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class ShareTransferStatistics
    {
        public int TotalTransfers { get; set; }
        public decimal TotalAmountTransferred { get; set; }
        public decimal AverageTransferAmount { get; set; }

        public int ActiveShareholders { get; set; }
        public int TransfersThisMonth { get; set; }
    }
    public class ShareTransferFilter
    {
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}