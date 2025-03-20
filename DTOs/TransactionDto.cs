namespace FinancialTransactionsManagementAPI.DTOs
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public CustomerDto Customer { get; set; } 
            //= new CustomerDto();
    }

    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateTransactionDto
    {
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        //public DateTime? TransactionDate { get; set; }
        public string Description { get; set; }
        //public string Status { get; set; }
        public CreateCustomerDto Customer { get; set; }
    }

    public class CreateCustomerDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
    }

    public class UpdateTransactionDto
    {
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
    }
}