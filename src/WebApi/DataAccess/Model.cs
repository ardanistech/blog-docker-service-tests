using ArdanisDockerizedServiceTests.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArdanisDockerizedServiceTests.WebApi.DataAccess
{
    public class ExpensesContext : DbContext
    {
        public DbSet<Expense> Expenses { get; set; }

        public ExpensesContext(DbContextOptions<ExpensesContext> context) : base(context)
        {

        }
    }
}
