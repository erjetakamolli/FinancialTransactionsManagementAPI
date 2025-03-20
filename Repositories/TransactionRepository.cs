using FinancialTransactionsManagementAPI.Models;
using FinancialTransactionsManagementAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactionsManagementAPI.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(string customerName = null, DateTime? startDate = null, DateTime? endDate = null, TransactionType? transactionType = null)
        {
            var query = _context.Transactions.Include(t => t.Customer).AsQueryable();

            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(t => t.Customer.FullName.Contains(customerName));
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            if (transactionType.HasValue)
            {
                query = query.Where(t => t.TransactionType == transactionType.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.TransactionId == id);
        }

        //public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        //{
        //    _context.Transactions.Add(transaction);
        //    await _context.SaveChangesAsync();
        //    return transaction;
        //}
        public async Task<decimal> GetCustomerBalanceAsync(int customerId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.CustomerId == customerId && t.Status == TransactionStatus.Successful)
                .ToListAsync();

            decimal balance = 0;
            foreach (var transaction in transactions)
            {
                if (transaction.TransactionType == TransactionType.Credit)
                {
                    balance += transaction.Amount;
                }
                else if (transaction.TransactionType == TransactionType.Debit)
                {
                    balance -= transaction.Amount;
                }
            }

            return balance;
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            transaction.TransactionDate = DateTime.Now;

            // Merr klientin nga databaza
            var customer = await _context.Customers.FindAsync(transaction.CustomerId);
            if (customer == null)
            {
                throw new Exception("Customer not found.");
            }

            if (transaction.Amount <= 0)
            {
                transaction.Status = TransactionStatus.Failed; // Shuma e pavlefshme
            }
            else if (transaction.TransactionType == TransactionType.Debit)
            {
                decimal balance = await GetCustomerBalanceAsync(transaction.CustomerId);
                if (balance < transaction.Amount)
                {
                    transaction.Status = TransactionStatus.Failed; 
                }
                else
                {
                    transaction.Status = TransactionStatus.Successful;
                }
            }
            else if (transaction.TransactionType == TransactionType.Credit)
            {
                transaction.Status = TransactionStatus.Successful;
            }

            // Ruaj transaksionin
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<bool> UpdateTransactionAsync(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TransactionSummary> GetTransactionSummaryAsync(string customerName = null, DateTime? startDate = null, DateTime? endDate = null, TransactionType? transactionType = null)
        {
            var transactions = await GetAllTransactionsAsync(customerName, startDate, endDate, transactionType);

            var summary = new TransactionSummary
            {
                TotalTransactions = transactions.Count(),
                TotalCredits = transactions.Where(t => t.TransactionType == TransactionType.Credit && t.Status == TransactionStatus.Successful).Sum(t => t.Amount),
                TotalDebits = transactions.Where(t => t.TransactionType == TransactionType.Debit && t.Status == TransactionStatus.Successful).Sum(t => t.Amount),
            };

            summary.NetBalance = summary.TotalCredits - summary.TotalDebits;

            return summary;
        }
    }
}