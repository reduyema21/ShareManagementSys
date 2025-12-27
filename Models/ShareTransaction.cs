using System.ComponentModel.DataAnnotations;

namespace SaccoShareManagementSys.Models
{
    public class ShareTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int ShareId { get; set; }
        public Share? Share { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public string TransactionType { get; set; } = "";
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? Description { get; set; }
        // Amount transferred or bought or deposited
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //// "Deposit", "Withdraw", "Transfer"
        //public string TransactionType { get; set; } = string.Empty;
    }
}
