using SaccoShareManagementSys.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaccoShareManagementSys.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        [Display(Name = "Share ID")]
        public int ShareId { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [StringLength(20)]
        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount (ETB)")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Completed";

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ShareId")]
        public virtual Share? Share { get; set; }
    }
}