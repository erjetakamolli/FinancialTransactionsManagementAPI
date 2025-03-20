using System;

namespace FinancialTransactionsManagementAPI.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}