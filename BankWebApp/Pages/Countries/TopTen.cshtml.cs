using AutoMapper;
using BankWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BankWebApp.Pages.Countries
{
    public class TopTenModel : PageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public TopTenModel(ICustomerService customerService, IAccountService accountService, IMapper mapper)
            => (_customerService, _accountService, _mapper) = (customerService, accountService, mapper);

        public List<CustomerCountryViewModel> Top10Customers { get; set; } = new List<CustomerCountryViewModel>();
        public string Flag { get; set; }

        public class CustomerCountryViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Emailaddress { get; set; }
            public decimal Balance { get; set; }
        }

        public async Task OnGetAsync(string countryId)
        {
            Top10Customers = _customerService.GetCustomers()
            .AsQueryable()
            .Include(c => c.Dispositions)
            .ThenInclude(d => d.Account)
            .Where(c => c.CountryCode == countryId)
            .SelectMany(c => c.Dispositions)
            .OrderByDescending(d => d.Account.Balance)
            .Take(10)
            .Select(d => new CustomerCountryViewModel
            {
                Id = d.CustomerId,
                Name = d.Customer.Surname + " " + d.Customer.Givenname,
                Emailaddress = d.Customer.Emailaddress,
                Balance = d.Account.Balance,
            })
            .ToList();

            Flag = countryId switch
            {
                "SE" => "/assets/img/flags/sweden-flag-png-large.png",
                "FI" => "/assets/img/flags/finland-flag-png-large.png",
                "DK" => "/assets/img/flags/denmark-flag-png-large.png",
                "NO" => "/assets/img/flags/norway-flag-png-large.png",
                _ => null,
            };
        }
    }

}
