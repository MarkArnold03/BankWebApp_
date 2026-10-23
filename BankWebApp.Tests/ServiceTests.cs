using AutoMapper;
using BankWebApp.BankAppData;
using BankWebApp.Pages.Customers;
using BankWebApp.Services;
using Xunit;

namespace BankWebApp.Tests
{
    /// <summary>The app's services against a real relational database built from its EF model.</summary>
    public class ServiceTests : IDisposable
    {
        private readonly TestDatabase _database = new();

        public void Dispose() => _database.Dispose();

        private decimal Balance(int accountId)
        {
            using var db = _database.CreateContext();
            return db.Accounts.Single(a => a.AccountId == accountId).Balance;
        }

        private Transaction LatestTransaction(int accountId)
        {
            using var db = _database.CreateContext();
            return db.Transactions.Where(t => t.AccountId == accountId).OrderByDescending(t => t.TransactionId).First();
        }

        private int TransactionCount()
        {
            using var db = _database.CreateContext();
            return db.Transactions.Count();
        }

        // ---------- AutoMapper ----------

        [Fact]
        public void AutoMapper_profiles_are_valid() =>
            new MapperConfiguration(c => c.AddMaps(typeof(Program).Assembly)).AssertConfigurationIsValid();

        // ---------- Deposits, withdrawals, transfers ----------

        [Fact]
        public void Deposit_is_recorded_as_a_credit_and_raises_the_balance()
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);
            service.Deposit(service.GetAccountForCustomer(TestDatabase.AnnaMain)!, 500.25m);

            Assert.Equal(10_500.25m, Balance(TestDatabase.AnnaMain));
            var t = LatestTransaction(TestDatabase.AnnaMain);
            Assert.Equal(("Credit", "Credit in Cash", 500.25m, 10_500.25m), (t.Type, t.Operation, t.Amount, t.Balance));
        }

        [Fact]
        public void Withdrawal_is_recorded_as_a_debit_and_lowers_the_balance()
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);
            service.Withdraw(service.GetAccountForCustomer(TestDatabase.AnnaMain)!, 10_000m);

            Assert.Equal(0m, Balance(TestDatabase.AnnaMain)); // the whole balance may be withdrawn
            var t = LatestTransaction(TestDatabase.AnnaMain);
            Assert.Equal(("Debit", "Withdrawal in Cash", -10_000m, 0m), (t.Type, t.Operation, t.Amount, t.Balance));
        }

        [Fact]
        public void Withdrawal_of_more_than_the_balance_is_refused_and_changes_nothing()
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);
            var before = TransactionCount();

            Assert.Throws<InvalidOperationException>(() => service.Withdraw(service.GetAccountForCustomer(TestDatabase.AnnaMain)!, 10_000.01m));
            Assert.Equal(10_000m, Balance(TestDatabase.AnnaMain));
            Assert.Equal(before, TransactionCount());
        }

        [Fact]
        public void Transfer_moves_money_and_records_both_sides()
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);
            service.Transfer(service.GetAccountForCustomer(TestDatabase.AnnaMain)!, service.GetAccountForCustomer(TestDatabase.OlaMain)!, 1_250.50m);

            Assert.Equal(8_749.50m, Balance(TestDatabase.AnnaMain));
            Assert.Equal(8_750.50m, Balance(TestDatabase.OlaMain));
            var debit = LatestTransaction(TestDatabase.AnnaMain);
            var credit = LatestTransaction(TestDatabase.OlaMain);
            Assert.Equal(("Debit", -1_250.50m, "2001"), (debit.Type, debit.Amount, debit.Account));
            Assert.Equal(("Credit", 1_250.50m, "1001"), (credit.Type, credit.Amount, credit.Account));
        }

        [Fact]
        public void Transfers_that_would_overdraw_or_go_to_the_same_account_are_refused()
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);
            var anna = service.GetAccountForCustomer(TestDatabase.AnnaMain)!;
            var ola = service.GetAccountForCustomer(TestDatabase.OlaMain)!;
            var before = TransactionCount();

            Assert.Throws<InvalidOperationException>(() => service.Transfer(anna, ola, 20_000m));
            Assert.Throws<InvalidOperationException>(() => service.Transfer(anna, anna, 100m));
            Assert.Equal((10_000m, 7_500m), (Balance(TestDatabase.AnnaMain), Balance(TestDatabase.OlaMain)));
            Assert.Equal(before, TransactionCount());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(10.005)] // fractions of an öre would cause rounding errors
        public void Invalid_amounts_are_refused(decimal amount)
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);
            var account = service.GetAccountForCustomer(TestDatabase.AnnaMain)!;

            Assert.Throws<ArgumentOutOfRangeException>(() => service.Deposit(account, amount));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Withdraw(account, amount));
            Assert.Equal(10_000m, Balance(TestDatabase.AnnaMain));
        }

        [Fact]
        public void Many_small_transactions_add_up_exactly()
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);
            var account = service.GetAccountForCustomer(TestDatabase.Shared)!;
            for (var i = 0; i < 10; i++) service.Deposit(account, 0.10m);
            for (var i = 0; i < 3; i++) service.Withdraw(account, 0.01m);

            Assert.Equal(2_000.97m, Balance(TestDatabase.Shared));
        }

        // ---------- Reading accounts and transactions ----------

        [Fact]
        public void Transactions_come_newest_first_in_batches_without_gaps_or_repeats()
        {
            using var db = _database.CreateContext();
            var service = new AccountService(db);

            var first = service.GetAccountTransactions(TestDatabase.AnnaMain, 0, 20);
            var second = service.GetAccountTransactions(TestDatabase.AnnaMain, 20, 20);
            var third = service.GetAccountTransactions(TestDatabase.AnnaMain, 40, 20);

            Assert.Equal((20, 20, 5), (first.Count, second.Count, third.Count));
            var all = first.Concat(second).Concat(third).ToList();
            Assert.Equal(45, all.Select(t => t.TransactionId).Distinct().Count());
            Assert.True(all.Zip(all.Skip(1)).All(p => p.First.Date >= p.Second.Date));
        }

        [Fact]
        public void Unknown_account_is_null() =>
            Assert.Null(new AccountService(_database.CreateContext()).GetAccountForCustomer(999_999));

        [Fact]
        public void A_customer_has_their_own_and_shared_accounts()
        {
            using var db = _database.CreateContext();
            var accounts = new AccountService(db).GetAccountsForCustomer(TestDatabase.AnnaId).Select(a => a.AccountId);

            Assert.Equal(new[] { TestDatabase.AnnaMain, TestDatabase.AnnaSavings, TestDatabase.Shared }, accounts);
        }

        [Theory]
        [InlineData("Balance", "desc", new[] { 1002, 1001, 2001, 1003 })]
        [InlineData("Balance", "asc", new[] { 1003, 2001, 1001, 1002 })]
        [InlineData("", "asc", new[] { 1001, 1002, 1003, 2001 })]
        [InlineData("AccountId", "desc", new[] { 2001, 1003, 1002, 1001 })]
        public void Accounts_sort_by_the_chosen_column(string column, string order, int[] expected)
        {
            using var db = _database.CreateContext();
            var page = new AccountService(db).GetAccounts(column, order, "", 1);

            Assert.Equal(expected, page.Results.Select(a => a.AccountId));
        }

        // ---------- Customers ----------

        [Theory]
        [InlineData("anna", new[] { "Anna" })]
        [InlineData("LINDQVIST", new[] { "Anna", "Erik" })]
        [InlineData("lindqvist uppsala", new[] { "Anna" })]
        [InlineData("oslo", new[] { "Ola" })]
        [InlineData("nobody", new string[0])]
        public void Customer_search_matches_every_word_against_name_and_city(string text, string[] expected)
        {
            using var db = _database.CreateContext();
            var page = new CustomerService(db).GetCustomers(0, "", "asc", text, 1);

            Assert.Equal(expected, page.Results.Select(c => c.Givenname).OrderBy(n => n));
        }

        [Fact]
        public void Customer_search_finds_a_customer_number()
        {
            using var db = _database.CreateContext();
            var page = new CustomerService(db).GetCustomers(0, "", "asc", TestDatabase.OlaId.ToString(), 1);

            Assert.Equal("Ola", Assert.Single(page.Results).Givenname);
        }

        [Fact]
        public void Customers_page_in_fifties_in_a_stable_order()
        {
            using (var db = _database.CreateContext())
            {
                for (var i = 0; i < 110; i++)
                    db.Customers.Add(new Customer { Gender = "male", Givenname = "Test", Surname = $"P{i:000}", Streetaddress = "Väg 1", City = "Malmö", Zipcode = "21100", Country = "Sweden", CountryCode = "SE" });
                db.SaveChanges();
            }

            using var ctx = _database.CreateContext();
            var service = new CustomerService(ctx);
            var pages = Enumerable.Range(1, 3).Select(p => service.GetCustomers(0, "", "asc", "malmö", p)).ToList();

            Assert.Equal((110, 3), (pages[0].RowCount, pages[0].PageCount));
            Assert.Equal(new[] { 50, 50, 10 }, pages.Select(p => p.Results.Count));
            Assert.Equal(110, pages.SelectMany(p => p.Results).Select(c => c.CustomerId).Distinct().Count());
        }

        [Fact]
        public void Unknown_customer_is_null() =>
            Assert.Null(new CustomerService(_database.CreateContext()).GetCustomer(999_999));

        [Theory]
        [InlineData("1980-06-15", "2026-06-14", 45)]
        [InlineData("1980-06-15", "2026-06-15", 46)]
        public void Age_counts_the_birthday_only_once_it_has_passed(string born, string today, int expected) =>
            Assert.Equal(expected, CustomerCardModel.AgeOn(DateTime.Parse(born), DateTime.Parse(today)));

        // ---------- Statistics ----------

        [Fact]
        public void Country_statistics_count_owners_once_and_sum_their_balances()
        {
            using var db = _database.CreateContext();
            var service = new CountryDataService(new CustomerService(db), db);

            Assert.Equal(2, service.GetCountryCustomersCount("SE"));
            Assert.Equal(3, service.GetCountryAccountsCount("SE"));        // shared account 1003 counted once
            Assert.Equal(62_000m, service.GetCountryBalance("SE"));        // and its balance once
            Assert.Equal((1, 1, 7_500m), (service.GetCountryCustomersCount("NO"), service.GetCountryAccountsCount("NO"), service.GetCountryBalance("NO")));
            Assert.Equal((3, 4, 69_500m), service.GetBankTotals());
        }
    }
}
