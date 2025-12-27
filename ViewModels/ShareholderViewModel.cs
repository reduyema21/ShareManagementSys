using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SaccoShareManagementSys.ViewModels
{
    public class ShareholderViewModel
    {
        public int ShareholderId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name *")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address *")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number *")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Join Date *")]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Display(Name = "Total Shares")]
        public decimal TotalShares { get; set; } = 0;

        [Display(Name = "Current Balance")]
        public decimal CurrentBalance { get; set; } = 0;

        [Display(Name = "Number of Certificates")]
        public int NumberOfCertificates { get; set; } = 0;

        [Required]
        [Display(Name = "Status *")]
        public string Status { get; set; } = "Active";

        [Required]
        [Display(Name = "Member Type *")]
        public string MemberType { get; set; } = "New";

        [StringLength(200)]
        [Display(Name = "Physical Address")]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(20)]
        [Display(Name = "ID/Passport Number")]
        public string? IdNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }
        public string? UserId { get; set; }

        // Dropdown lists
        public SelectList? StatusList { get; set; }
        public SelectList? MemberTypeList { get; set; }
        public SelectList? GenderList { get; set; }
    }
    public class ShareholderIndexViewModel
    {
        public List<ShareholderListItem> Shareholders { get; set; } = new();
        public ShareholderStatistics Statistics { get; set; } = new();
        public ShareholderSearchFilter Filter { get; set; } = new();
    }

    public class ShareholderListItem
    {
        public int ShareholderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public decimal TotalShares { get; set; }
        public decimal CurrentBalance { get; set; }
        public int NumberOfCertificates { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
    }

    public class ShareholderStatistics
    {
        public int TotalShareholders { get; set; }
        public int ActiveShareholders { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public decimal AverageBalance { get; set; }
    }

    public class ShareholderSearchFilter
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? MemberType { get; set; }
    }

}
