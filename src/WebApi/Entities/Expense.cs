using System;

namespace ArdanisDockerizedServiceTests.WebApi.Entities
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public Guid ExternalId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string ReceiptFilename { get; set; }
    }
}
