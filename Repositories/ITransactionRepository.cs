using System;

using FinancialTransactionsManagementAPI.Models;

namespace FinancialTransactionsManagementAPI.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync(string customerName = null, DateTime ? startDate = null, DateTime? endDate = null, TransactionType? transactionType = null);
        Task<Transaction> GetTransactionByIdAsync(int id);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<bool> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(int id);
        Task<TransactionSummary> GetTransactionSummaryAsync(string customerName = null, DateTime? startDate = null, DateTime? endDate = null, TransactionType? transactionType = null);

        Task<decimal> GetCustomerBalanceAsync(int customerId);
    }

    public class TransactionSummary
    {
        public int TotalTransactions { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal NetBalance { get; set; }
    }
}