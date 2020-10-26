using ArdanisDockerizedServiceTests.WebApi.DataAccess;
using ArdanisDockerizedServiceTests.WebApi.Entities;
using ArdanisDockerizedServiceTests.WebApi.Requests;
using ArdanisDockerizedServiceTests.WebApi.Responses;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArdanisDockerizedServiceTests.WebApi.Controllers
{
    [ApiController]
    [Route("/api/expenses")]
    public class ExpensesController : ControllerBase
    {
        private readonly ExpensesContext _dbContext;
        private readonly BlobServiceClient _blobServiceClient;

        public ExpensesController(ExpensesContext dbContext, BlobServiceClient blobServiceClient)
        {
            _dbContext = dbContext;
            _blobServiceClient = blobServiceClient;
        }

        [HttpPost]
        public async Task<IActionResult> PostExpense([FromBody] PostExpenseRequest request)
        {
            _dbContext.Add(new Expense()
            {
                Amount = request.Amount,
                Description = request.Description,
                ExternalId = request.ExpenseId
            });

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("{expenseId}/receipts")]
        public async Task<IActionResult> PostExpenseFile(Guid expenseId, List<IFormFile> receipts)
        {
            var expense = await _dbContext.Expenses.FirstOrDefaultAsync(x => x.ExternalId == expenseId);
            if (expense == null)
                return NotFound();
            foreach (var receipt in receipts)
            {
                expense.ReceiptFilename = receipt.FileName;
                await StoreFileAsync(expense, receipt);
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        private async Task StoreFileAsync(Expense expense, IFormFile receipt)
        {
            var containerClient = await GetBlobClientAsync(expense);
            using (var s = receipt.OpenReadStream())
            {
                await containerClient.UploadBlobAsync(receipt.FileName, s);
            }
        }

        private async Task<BlobContainerClient> GetBlobClientAsync(Expense expense)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(expense.ExternalId.ToString());
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
            return containerClient;
        }

        [HttpGet]
        [Route("{expenseId}")]
        public async Task<IActionResult> GetExpense(Guid expenseId)
        {
            var expense = await _dbContext.Expenses.SingleOrDefaultAsync(x => x.ExternalId == expenseId);

            if (expense == null)
            {
                return NotFound();
            }

            var response = new GetExpenseResponse()
            {
                Amount = expense.Amount,
                Description = expense.Description,
                ExpenseId = expense.ExternalId,
                ReceiptFilename = expense.ReceiptFilename
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("{expenseId}/receipts")]
        public async Task<IActionResult> DownloadExpenseReceipt(Guid expenseId)
        {
            var expense = await _dbContext.Expenses.SingleOrDefaultAsync(x => x.ExternalId == expenseId);

            if (expense == null || string.IsNullOrEmpty(expense.ReceiptFilename))
            {
                return NotFound();
            }
            var client = await GetBlobClientAsync(expense);
            var file = await client.GetBlobClient(expense.ReceiptFilename).DownloadAsync();

            return File(file.Value.Content, "application/octet-stream");
        }
    }


}
