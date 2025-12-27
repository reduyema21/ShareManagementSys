namespace SaccoShareManagementSys.Models
{
    public class ShareTransferReviewViewModel
    {
        public int SenderId { get; set; }
        public string? SenderName { get; set; }
        public decimal SenderCurrentBalance { get; set; }
        public decimal SenderBalanceAfter { get; set; }

        public int ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public decimal ReceiverCurrentBalance { get; set; }
        public decimal ReceiverBalanceAfter { get; set; }

        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
