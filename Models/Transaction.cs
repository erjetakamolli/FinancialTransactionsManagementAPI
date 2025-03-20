using System;

namespace FinancialTransactionsManagementAPI.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }

        // Foreign key
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}