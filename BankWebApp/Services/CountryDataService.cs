using BankWebApp.BankAppData;
using BankWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BankWebApp.Services
{
    public class CountryDataService : ICountryDataService
    {
        private readonly ICustomerService _customerService;

        public CountryDataService(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public int GetCountryCustomersCount(string country)
        {
            var customers = _customerService.GetCustomers()
                .Where(c => c.CountryCode == country);
            return customers.Count();
        }

        public int GetCountryAccountsCount(string country)
        {
            var dispSE = _customerService.GetCustomers()
                .AsQueryable()
                .Include(c => c.Dispositions)
                .ThenInclude(d => d.Account)
                .Where(c => c.CountryCode == country)
                .SelectMany(c => c.Dispositions.Where(c => c.Type == "OWNER")); // Count only "Owners" of account (not joint accounts)
            return dispSE.Count();
        }

        public decimal GetCountryBalance(string country)
        {
            var dispSE = _customerService.GetCustomers()
                .AsQueryable()
                .Include(c => c.Dispositions)
                .ThenInclude(d => d.Account)
                .Where(c => c.CountryCode == country)
                .SelectMany(c => c.Dispositions);

            return dispSE
                .Select(d => d.Account.Balance)
                .Sum();
        }
    }


}
