using System;
using Microsoft.AspNetCore.Http;

namespace ArdanisDockerizedServiceTests.WebApi.Requests
{
    public class PostExpenseRequest
    {
        public Guid ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
