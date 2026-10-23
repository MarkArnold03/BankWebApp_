using BankWebApp.BankAppData;

namespace BankWebApp.Services
{
    public class CountryDataService : ICountryDataService
    {
        private readonly ICustomerService _customerService;
        private readonly BankAppDataContext _context;

        public CountryDataService(ICustomerService customerService, BankAppDataContext context)
        {
            _customerService = customerService;
            _context = context;
        }

        public int GetCountryCustomersCount(string country)
        {
            return _customerService.GetCustomers().Count(c => c.CountryCode == country);
        }

        public int GetCountryAccountsCount(string country)
        {
            // Count only owners of accounts (not joint holders), so each account is counted once.
            return _customerService.GetCustomers()
                .Where(c => c.CountryCode == country)
                .SelectMany(c => c.Dispositions.Where(d => d.Type == "OWNER"))
                .Count();
        }

        public decimal GetCountryBalance(string country)
        {
            var balances = _customerService.GetCustomers()
                .Where(c => c.CountryCode == country)
                .SelectMany(c => c.Dispositions.Where(d => d.Type == "OWNER"))
                .Select(d => d.Account.Balance);
            return Sum(balances);
        }

        public (int Customers, int Accounts, decimal TotalBalance) GetBankTotals()
        {
            return (_context.Customers.Count(), _context.Accounts.Count(), Sum(_context.Accounts.Select(a => a.Balance)));
        }

        // SQL Server sums decimals in the database. SQLite (used by the tests) can't, so it sums in memory.
        private decimal Sum(IQueryable<decimal> values) =>
            _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer" ? values.Sum() : values.AsEnumerable().Sum();
    }
}
