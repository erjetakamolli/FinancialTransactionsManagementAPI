using Microsoft.AspNetCore.Mvc;
using FinancialTransactionsManagementAPI.Repositories;
using FinancialTransactionsManagementAPI.DTOs;
using FinancialTransactionsManagementAPI.Models;
using FinancialTransactionsManagementAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialTransactionsManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionRepository _repository;
        private readonly AppDbContext _context; 

        public TransactionsController(ITransactionRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context; // Inicializimi i DbContext
        }

        // GET: api/transactions
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions(
        //    [FromQuery] string customerName = null,
        //    [FromQuery] DateTime? startDate = null,
        //    [FromQuery] DateTime? endDate = null,
        //    [FromQuery] string transactionType = null)
        //{
        //    // Convert string parameters to enums if they're provided
        //    TransactionType? transactionTypeEnum = !string.IsNullOrEmpty(transactionType)
        //    ? Enum.Parse<TransactionType>(transactionType)
        //    : null;

        //    var transactions = await _repository.GetAllTransactionsAsync(customerName, startDate, endDate, transactionTypeEnum);
        //    var transactionDtos = transactions.Select(t => new TransactionDto
        //    {
        //        TransactionId = t.TransactionId,
        //        Amount = t.Amount,
        //        TransactionType = t.TransactionType.ToString(),
        //        TransactionDate = t.TransactionDate,
        //        Description = t.Description,
        //        Status = t.Status.ToString(),
        //        Customer = new CustomerDto
        //        {
        //            CustomerId = t.Customer.CustomerId,
        //            FullName = t.Customer.FullName,
        //            PhoneNumber = t.Customer.PhoneNumber,
        //            Address = t.Customer.Address,
        //            Email = t.Customer.Email
        //        }
        //    });

        //    return Ok(transactionDtos);
        //}
        // GET: api/transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions(
            [FromQuery] string? customerName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? transactionType = null)
        {
            if (string.IsNullOrEmpty(customerName) &&
                !string.IsNullOrEmpty(transactionType) &&
                (transactionType.Equals("Credit", StringComparison.OrdinalIgnoreCase) ||
                 transactionType.Equals("Debit", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Please enter a customer name to view transactions of type 'Credit' or 'Debit'.");
            }

            TransactionType? transactionTypeEnum = !string.IsNullOrEmpty(transactionType)
                ? Enum.Parse<TransactionType>(transactionType, true)
                : null;

            var transactions = await _repository.GetAllTransactionsAsync(customerName, startDate, endDate, transactionTypeEnum);
            var transactionDtos = transactions.Select(t => new TransactionDto
            {
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                TransactionType = t.TransactionType.ToString(),
                TransactionDate = t.TransactionDate,
                Description = t.Description,
                Status = t.Status.ToString(),
                Customer = new CustomerDto
                {
                    CustomerId = t.Customer.CustomerId,
                    FullName = t.Customer.FullName,
                    PhoneNumber = t.Customer.PhoneNumber,
                    Address = t.Customer.Address,
                    Email = t.Customer.Email
                }
            });

            return Ok(transactionDtos);
        }

        // GET: api/transactions/id
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> GetTransactionById(int id)
        {
            var transaction = await _repository.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound();

            var transactionDto = new TransactionDto
            {
                TransactionId = transaction.TransactionId,
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType.ToString(),
                TransactionDate = transaction.TransactionDate,
                Description = transaction.Description,
                Status = transaction.Status.ToString(),
                Customer = new CustomerDto
                {
                    CustomerId = transaction.Customer.CustomerId,
                    FullName = transaction.Customer.FullName,
                    PhoneNumber = transaction.Customer.PhoneNumber,
                    Address = transaction.Customer.Address,
                    Email = transaction.Customer.Email
                }
            };

            return Ok(transactionDto);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kontrollon nese klienti ekziston ose krijon 1 te ri
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == createDto.Customer.Email);
            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = createDto.Customer.FullName,
                    PhoneNumber = createDto.Customer.PhoneNumber,
                    Address = createDto.Customer.Address,
                    Email = createDto.Customer.Email
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // Krijon transaksionin
            var transaction = new Transaction
            {
                Amount = createDto.Amount,
                TransactionType = Enum.Parse<TransactionType>(createDto.TransactionType),
                TransactionDate = DateTime.Now,
                Description = createDto.Description,
                Status = TransactionStatus.Successful, // Statusi fillestar 
                CustomerId = customer.CustomerId
            };

            string failureReason = null; // Mesazhi i failed

            if (transaction.Amount <= 0)
            {
                transaction.Status = TransactionStatus.Failed;
                failureReason = "The transaction amount must be greater than zero.";
            }
            else if (transaction.TransactionType == TransactionType.Debit)
            {
                decimal balance = await _repository.GetCustomerBalanceAsync(customer.CustomerId);
                if (balance < transaction.Amount)
                {
                    transaction.Status = TransactionStatus.Failed;
                    failureReason = "Insufficient funds in the account.";
                }
            }

            // Ruaj transaksionin në database edhe nëse është failed
            var createdTransaction = await _repository.CreateTransactionAsync(transaction);

            // Krijo DTO për transaksionin
            var createdTransactionDto = new TransactionDto
            {
                TransactionId = createdTransaction.TransactionId,
                Amount = createdTransaction.Amount,
                TransactionType = createdTransaction.TransactionType.ToString(),
                TransactionDate = createdTransaction.TransactionDate,
                Description = createdTransaction.Description,
                Status = createdTransaction.Status.ToString(),
                Customer = new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    Address = customer.Address,
                    Email = customer.Email
                }
            };

            // Nëse transaksioni ka dështuar, kthe një përgjigje me mesazhin e gabimit, por transaksioni është ruajtur
            if (transaction.Status == TransactionStatus.Failed)
            {
                return BadRequest(new
                {
                    message = "Transaction failed",
                    reason = failureReason,
                    transaction = createdTransactionDto // Kthe edhe transaksionin e ruajtur
                });
            }

            return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.TransactionId },
                  new { message = "Transaction created successfully", transaction = createdTransactionDto });
        }

        [HttpPut("void/{id}")]
        public async Task<IActionResult> VoidTransaction(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Ndryshon statusin ne Voided
            transaction.Status = TransactionStatus.Voided;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Status updated successfully in 'Voided'.",
                Transaction = transaction
            });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _repository.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound();

            // Perditeson fushat e transaksionit te caktuara
            transaction.Amount = updateDto.Amount;
            transaction.TransactionType = Enum.Parse<TransactionType>(updateDto.TransactionType);
            transaction.TransactionDate = DateTime.Now; // Vendos daten aktuale
            transaction.Description = updateDto.Description;

            if (transaction.Amount <= 0)
            {
                transaction.Status = TransactionStatus.Failed; // Shuma e pavlefshme
            }
            else if (transaction.TransactionType == TransactionType.Debit)
            {
                decimal balance = await _repository.GetCustomerBalanceAsync(transaction.CustomerId);
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

            var result = await _repository.UpdateTransactionAsync(transaction);
            if (!result)
                return BadRequest("Failed to update transaction");

            return NoContent();
        }
        // DELETE: api/transactions/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var result = await _repository.DeleteTransactionAsync(id);
            if (!result)
                return NotFound();

            // Kthe nje mesazh informues
            return Ok(new { message = "Transaction deleted successfully." });
        }

        [HttpGet("summary")]
        public async Task<ActionResult<TransactionSummary>> GetTransactionSummary(
        [FromQuery] string customerName = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string transactionType = null)
        {
            TransactionType? parsedTransactionType = null;

            if (!string.IsNullOrEmpty(transactionType) && Enum.TryParse<TransactionType>(transactionType, true, out var parsedValue))
            {
                parsedTransactionType = parsedValue;
            }

            var summary = await _repository.GetTransactionSummaryAsync(customerName, startDate, endDate, parsedTransactionType);
            return Ok(summary);
        }  

    }
}