using BankWebApp.BankAppData;
using BankWebApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BankWebApp.Tests
{
    /// <summary>
    /// Starts the real app (Program.cs, Identity, Razor Pages, EF, DataInitializer) with both
    /// databases on SQLite in memory instead of SQL Server, plus a login without a role.
    /// </summary>
    public class BankAppFactory : WebApplicationFactory<Program>
    {
        // The staff logins from the README, created by DataInitializer at startup.
        public const string AdminEmail = "richard.chalk@systementor.se";
        public const string CashierEmail = "richard.erdos.chalk@gmail.se";
        public const string Password = "Hejsan123#";
        public const string NoRoleEmail = "visitor@example.com";

        private readonly SqliteConnection _identityConnection = new("DataSource=:memory:");
        private readonly SqliteConnection _bankConnection = new("DataSource=:memory:");

        public BankAppFactory()
        {
            _identityConnection.Open();
            _bankConnection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Warning));
            builder.ConfigureServices(services =>
            {
                foreach (var t in new[] { typeof(DbContextOptions<ApplicationDbContext>), typeof(DbContextOptions<BankAppDataContext>) })
                    foreach (var d in services.Where(d => d.ServiceType == t).ToList()) services.Remove(d);
                services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_identityConnection));
                services.AddDbContext<BankAppDataContext>(o => o.UseTestSqlite(_bankConnection));

                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
                var bank = scope.ServiceProvider.GetRequiredService<BankAppDataContext>();
                if (bank.Database.EnsureCreated()) TestDatabase.Seed(bank);

                var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                if (users.FindByEmailAsync(NoRoleEmail).GetAwaiter().GetResult() == null)
                    users.CreateAsync(new IdentityUser { UserName = NoRoleEmail, Email = NoRoleEmail, EmailConfirmed = true }, Password).GetAwaiter().GetResult();
            });
        }

        public BankAppDataContext CreateBankContext() =>
            new((DbContextOptions<BankAppDataContext>)new DbContextOptionsBuilder<BankAppDataContext>().UseTestSqlite(_bankConnection).Options);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _identityConnection.Dispose();
                _bankConnection.Dispose();
            }
        }
    }
}
