
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNetCore.Identity;
namespace SaccoShareManagementSys.Models
{
    public class Shareholder
        {
            [Key]
            public int ShareholderId { get; set; }

            [Required(ErrorMessage = "Full name is required")]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [StringLength(100)]
            [Display(Name = "Email Address")]
            public string Email { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Invalid phone number")]
            [StringLength(20)]
            [Display(Name = "Phone Number")]
            public string? Phone { get; set; }  // nullable, remove [Required]


            [Required]
            [Display(Name = "Join Date")]
            [DataType(DataType.Date)]
            public DateTime JoinDate { get; set; } = DateTime.Now;

            [Required]
            [Column(TypeName = "decimal(18, 2)")]
            [Display(Name = "Total Shares")]
            public decimal TotalShares { get; set; } = 0;

            [Required]
            [Column(TypeName = "decimal(18, 2)")]
            [Display(Name = "Current Balance")]
            public decimal CurrentBalance { get; set; } = 0;

            [Display(Name = "Number of Certificates")]
            public int NumberOfCertificates { get; set; } = 0;

            [Required]
            [StringLength(20)]
            [Display(Name = "Status")]
            public string Status { get; set; } = "Active";

            [Required]
            [StringLength(20)]
            [Display(Name = "Member Type")]
            public string MemberType { get; set; } = "New";

            [StringLength(200)]
            [Display(Name = "Physical Address")]
            public string? Address { get; set; }

            [StringLength(50)]
            [Display(Name = "City")]
            public string? City { get; set; }

            [StringLength(20)]
            [Display(Name = "ID")]
            public string? IdNumber { get; set; }

            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            [StringLength(20)]
            [Display(Name = "Gender")]
            public string? Gender { get; set; }

            [Display(Name = "Created At")]
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            [Display(Name = "Updated At")]
            public DateTime? UpdatedAt { get; set; }
        
            [Display(Name = "Is Approved")]
            public bool IsApproved { get; set; } = false;

            public string? UserId { get; set; }

            public IdentityUser? User { get; set; }

            // Navigation properties
            public virtual ICollection<Share>? Shares { get; set; }
            public virtual ICollection<ShareTransfer>? TransfersFrom { get; set; }
            public virtual ICollection<ShareTransfer>? TransfersTo { get; set; }
    }
}

