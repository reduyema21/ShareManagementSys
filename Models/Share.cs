using SaccoShareManagementSys.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaccoShareManagementSys.Models
{
    public class Share
    {
        [Key]
        public int ShareId { get; set; }

        [Required]
        [Display(Name = "Shareholder")]
        public int ShareholderId { get; set; }

        [Required(ErrorMessage = "Certificate number is required")]
        [StringLength(50)]
        [Display(Name = "Certificate Number")]
        public string CertificateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Share type is required")]
        [StringLength(50)]
        [Display(Name = "Share Type")]
        public string ShareType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Number of Shares")]
        public decimal NumberOfShares { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Share Value (ETB)")]
        public decimal ShareValue { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        [StringLength(200)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Maturity Date")]
        [DataType(DataType.Date)]
        public DateTime? MaturityDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ShareholderId")]
        public virtual Shareholder? Shareholder { get; set; }

        public virtual ICollection<ShareTransaction>? Transactions { get; set; } = new List<ShareTransaction>();
    }
}