using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaccoShareManagementSys.Models
{
    public class Dividend
    {
        [Key]
        public int DividendId { get; set; }

        [Required]
        [Display(Name = "Dividend Year")]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Dividend Month")]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total Profit (ETB)")]
        public decimal TotalProfit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total Shares")]
        public decimal TotalShares { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 4)")]
        [Display(Name = "Dividend Rate (%)")]
        public decimal DividendRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total Dividend Paid (ETB)")]
        public decimal TotalDividendPaid { get; set; }

        [Required]
        [Display(Name = "Distribution Date")]
        [DataType(DataType.Date)]
        public DateTime DistributionDate { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // foreign key for shareholder
        public int? ShareholderId { get; set; }

        [ForeignKey("ShareholderId")]
        public virtual Shareholder? Shareholder { get; set; }
    }
}