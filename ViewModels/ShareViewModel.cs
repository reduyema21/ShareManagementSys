using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SaccoShareManagementSys.ViewModels
{
    public class ShareViewModel
    {

        public int ShareId { get; set; }

        [Required(ErrorMessage = "Please select a shareholder")]
        [Display(Name = "Shareholder *")]
        public int ShareholderId { get; set; }

        [Required(ErrorMessage = "Certificate number is required")]
        [StringLength(50)]
        [Display(Name = "Certificate Number *")]
        public string CertificateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Share type is required")]
        [Display(Name = "Share Type *")]
        public string ShareType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Number of shares is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Number of shares must be greater than 0")]
        [Display(Name = "Number of Shares *")]
        public decimal NumberOfShares { get; set; }

        [Required(ErrorMessage = "Share value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Share value must be greater than 0")]
        [Display(Name = "Share Value (KES) *")]
        [DataType(DataType.Currency)]
        public decimal ShareValue { get; set; }

        [Required]
        [Display(Name = "Purchase Date *")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Status *")]
        public string Status { get; set; } = "Active";

        [StringLength(200)]
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Maturity Date")]
        [DataType(DataType.Date)]
        public DateTime? MaturityDate { get; set; }

        // Additional properties for display
        public string? ShareholderName { get; set; }
        public decimal TotalValue => NumberOfShares * ShareValue;

        // Dropdown lists
        public SelectList? Shareholders { get; set; }
        public SelectList? ShareTypes { get; set; }
        public SelectList? StatusList { get; set; }
    }

    public class ShareIndexViewModel
    {

        public List<ShareListItem> Shares { get; set; } = new();
        public ShareStatistics Statistics { get; set; } = new();
        public ShareSearchFilter Filter { get; set; } = new();
    }

    public class ShareListItem
    {
        public int ShareId { get; set; }
        public int ShareholderId { get; set; }
        public string ShareholderName { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;

        public string ShareType { get; set; } = string.Empty;
        public decimal NumberOfShares { get; set; }
        public decimal ShareValue { get; set; }

        public decimal InitialAmount { get; set; }    // Could be NumberOfShares * ShareValue at purchase
        public decimal CurrentBalance { get; set; }   // Could come from related Transactions

        public decimal GrowthPercent { get; set; }   // Percentage gain
        public int TransactionCount { get; set; }     // Count of related Transactions

        public decimal TotalValue { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ShareStatistics
    {
        public int TotalCertificates { get; set; }
        public int ActiveCertificates { get; set; }
        public decimal TotalShareValue { get; set; }
        public decimal TotalShares { get; set; }
        public decimal AverageShareValue { get; set; }
    }

    public class ShareSearchFilter
    {
        public string? SearchTerm { get; set; }
        public string? ShareType { get; set; }
        public string? Status { get; set; }
        public int? ShareholderId { get; set; }
    }
}