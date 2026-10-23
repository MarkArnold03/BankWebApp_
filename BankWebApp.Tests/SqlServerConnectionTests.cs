using BankWebApp.BankAppData;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankWebApp.Tests
{
    /// <summary>Runs only when BANKAPP_SQLSERVER is set to a connection string.</summary>
    public sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BANKAPP_SQLSERVER")))
                Skip = "Set BANKAPP_SQLSERVER to the BankAppData connection string to test the real database.";
        }
    }

    /// <summary>Checks the EF model against your real SQL Server database.</summary>
    public class SqlServerConnectionTests
    {
        private static BankAppDataContext Connect() =>
            new(new DbContextOptionsBuilder<BankAppDataContext>().UseSqlServer(Environment.GetEnvironmentVariable("BANKAPP_SQLSERVER")!).Options);

        [SqlServerFact]
        public async Task Can_connect()
        {
            using var db = Connect();
            Assert.True(await db.Database.CanConnectAsync());
        }

        [SqlServerFact]
        public async Task Every_table_used_by_the_app_matches_the_model()
        {
            using var db = Connect();
            // Selecting one full row per table makes SQL Server check every mapped column.
            await db.Customers.AsNoTracking().FirstOrDefaultAsync();
            await db.Accounts.AsNoTracking().FirstOrDefaultAsync();
            await db.Dispositions.AsNoTracking().FirstOrDefaultAsync();
            await db.Transactions.AsNoTracking().FirstOrDefaultAsync();
            await db.Cards.AsNoTracking().FirstOrDefaultAsync();
            await db.Loans.AsNoTracking().FirstOrDefaultAsync();
            await db.PermenentOrders.AsNoTracking().FirstOrDefaultAsync();
        }

        [SqlServerFact]
        public async Task Bank_has_customers_and_accounts()
        {
            using var db = Connect();
            Assert.True(await db.Customers.AnyAsync(), "No customers.");
            Assert.True(await db.Accounts.AnyAsync(), "No accounts.");
        }
    }
}
