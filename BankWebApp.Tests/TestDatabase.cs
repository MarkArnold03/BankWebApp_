using BankWebApp.BankAppData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BankWebApp.Tests
{
    /// <summary>
    /// A real relational database (SQLite in memory) built from the app's own EF model.
    /// Two Swedish customers and one Norwegian; Anna and Erik share account 1003.
    /// </summary>
    public sealed class TestDatabase : IDisposable
    {
        public const int AnnaMain = 1001, AnnaSavings = 1002, Shared = 1003, OlaMain = 2001;
        public static int AnnaId, ErikId, OlaId;

        private readonly SqliteConnection _connection;

        public TestDatabase()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            using var db = CreateContext();
            db.Database.EnsureCreated();
            Seed(db);
        }

        public BankAppDataContext CreateContext() =>
            new((DbContextOptions<BankAppDataContext>)new DbContextOptionsBuilder<BankAppDataContext>().UseTestSqlite(_connection).Options);

        public static void Seed(BankAppDataContext db)
        {
            var anna = Customer("Anna", "Lindqvist", "Uppsala", "SE", "Sweden", new DateTime(1980, 6, 15));
            var erik = Customer("Erik", "Lindqvist", "Stockholm", "SE", "Sweden", null);
            var ola = Customer("Ola", "Nordmann", "Oslo", "NO", "Norway", new DateTime(1990, 1, 1));
            db.Customers.AddRange(anna, erik, ola);
            db.Accounts.AddRange(
                new Account { AccountId = AnnaMain, Frequency = "Monthly", Created = new DateTime(2019, 3, 14), Balance = 10_000m },
                new Account { AccountId = AnnaSavings, Frequency = "Monthly", Created = new DateTime(2021, 8, 2), Balance = 50_000m },
                new Account { AccountId = Shared, Frequency = "Weekly", Created = new DateTime(2023, 1, 9), Balance = 2_000m },
                new Account { AccountId = OlaMain, Frequency = "AfterTransaction", Created = new DateTime(2020, 5, 5), Balance = 7_500m });
            db.SaveChanges();
            (AnnaId, ErikId, OlaId) = (anna.CustomerId, erik.CustomerId, ola.CustomerId);

            db.Dispositions.AddRange(
                new Disposition { CustomerId = anna.CustomerId, AccountId = AnnaMain, Type = "OWNER" },
                new Disposition { CustomerId = anna.CustomerId, AccountId = AnnaSavings, Type = "OWNER" },
                new Disposition { CustomerId = erik.CustomerId, AccountId = Shared, Type = "OWNER" },
                new Disposition { CustomerId = anna.CustomerId, AccountId = Shared, Type = "DISPONENT" },
                new Disposition { CustomerId = ola.CustomerId, AccountId = OlaMain, Type = "OWNER" });

            var day = DateTime.Today;
            for (var i = 0; i < 45; i++)
            {
                var credit = i % 3 == 0;
                db.Transactions.Add(new Transaction
                {
                    AccountId = AnnaMain,
                    Date = day.AddDays(-i),
                    Type = credit ? "Credit" : "Debit",
                    Operation = credit ? "Collection from Another Bank" : "Credit Card Withdrawal",
                    Amount = credit ? 1_000m : -250m,
                    Balance = 10_000m,
                    Symbol = credit ? "Pension" : null,
                });
            }
            db.SaveChanges();
        }

        private static Customer Customer(string given, string sur, string city, string code, string country, DateTime? born) => new()
        {
            Gender = "female", Givenname = given, Surname = sur, Streetaddress = "Storgatan 1", City = city, Zipcode = "12345",
            Country = country, CountryCode = code, Birthday = born, NationalId = born?.ToString("yyyyMMdd") + "-1234",
            Emailaddress = $"{given.ToLower()}@example.com"
        };

        public void Dispose() => _connection.Dispose();
    }
}
