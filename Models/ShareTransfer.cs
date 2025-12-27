
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaccoShareManagementSys.Models
{
    public class ShareTransfer
    {
        [Key]
        public int TransferId { get; set; }

        [Required(ErrorMessage = "Sender is required")]
        [Display(Name = "From Shareholder")]
        public int FromShareholderId { get; set; }

        [Required(ErrorMessage = "Receiver is required")]
        [Display(Name = "To Shareholder")]
        public int ToShareholderId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Share Amount (ETB)")]
        public decimal ShareAmount { get; set; }

        [Required]
        [Display(Name = "Transfer Date")]
        [DataType(DataType.DateTime)]
        public DateTime TransferDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Completed";

        [MaxLength(500)]
        [Display(Name = "Transfer Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FromShareholderId")]
        public virtual Shareholder? FromShareholder { get; set; }

        [ForeignKey("ToShareholderId")]
        public virtual Shareholder? ToShareholder { get; set; }
    }
}