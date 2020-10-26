using System;
using System.Collections.Generic;

namespace ArdanisDockerizedServiceTests.WebApi.Responses
{
    public class GetExpenseResponse
    {
        public Guid ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string ReceiptFilename { get; set; }
    }
}
