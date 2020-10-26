using ArdanisDockerizedServiceTests.WebApi.Requests;
using ArdanisDockerizedServiceTests.WebApi.Responses;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace ArdanisDockerizedServiceTests.ServiceTests.Steps
{
    [Binding]
    public class ExpensesSteps
    {
        private HttpClient _client;
        private Guid _expenseId;
        private byte[] _receiptFileContents;

        public ExpensesSteps()
        {
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables(prefix: "EnvVars_")
                .Build();
            _client = new HttpClient();
            _client.BaseAddress = new Uri(configuration.GetValue<string>("webapi_url"));
        }

        [Given(@"a business expense")]
        public void GivenABusinessExpense()
        {
            //do some setup
        }
        
        [When(@"I submit the expense")]
        public async Task WhenISubmitTheExpense()
        {
            _expenseId = Guid.NewGuid();
            var json = JsonSerializer.Serialize(new PostExpenseRequest
            {
                Amount = 5,
                Description = "foo",
                ExpenseId = _expenseId,
            });
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/expenses", data);
        }

        [Then(@"the expense should be stored")]
        public async Task ThenTheExpenseShouldBeStored()
        {
            var response = await _client.GetAsync($"/api/expenses/{_expenseId}");
            response.EnsureSuccessStatusCode();
            var str = await response.Content.ReadAsStringAsync();
            var expense = JsonSerializer.Deserialize<GetExpenseResponse>(
                str, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(_expenseId, expense.ExpenseId);
        }

        [When(@"I submit a receipt")]
        public async Task AndISubmitAReceipt()
        {
            var data = new MultipartFormDataContent();
            _receiptFileContents = Guid.NewGuid().ToByteArray();
            data.Add(new ByteArrayContent(_receiptFileContents), "receipts", $"{Guid.NewGuid()}.png");
            var response = await _client.PostAsync($"/api/expenses/{_expenseId}/receipts", data);
            response.EnsureSuccessStatusCode();
        }

        [Then(@"the receipt should be available to download")]
        public async Task ThenTheReceiptShouldBeAvailableToDownload()
        {
            var response = await _client.GetAsync($"/api/expenses/{_expenseId}/receipts");
            response.EnsureSuccessStatusCode();
            await using var memoryStream = new MemoryStream();
            await (await response.Content.ReadAsStreamAsync()).CopyToAsync(memoryStream);
            var byteArray = memoryStream.ToArray();
            Assert.AreEqual(_receiptFileContents, byteArray);
        }
    }
}
