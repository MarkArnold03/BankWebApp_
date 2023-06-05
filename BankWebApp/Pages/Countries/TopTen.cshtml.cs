using AutoMapper;
using BankWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BankWebApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BankWebApp.Pages.Countries
{
    

    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "countryId" })]

    public class TopTenModel : PageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public TopTenModel(ICustomerService customerService, IAccountService accountService, IMapper mapper)
        {
            _customerService = customerService;
            _accountService = accountService;
            _mapper = mapper;
        }

        public IEnumerable<CountryViewModel> Top10Customers { get; set; }
        public string Flag { get; set; }

        public void OnGet(string countryId)
        {
            Top10Customers = _customerService.GetCustomers()
            .AsQueryable()
            .Include(c => c.Dispositions)
            .ThenInclude(d => d.Account)
            .Where(c => c.CountryCode == countryId)
            .SelectMany(c => c.Dispositions)
            .OrderByDescending(d => d.Account.Balance)
            .Take(10)
            .Select(d => new CountryViewModel
            {
                Id = d.CustomerId,
                Name = d.Customer.Surname + " " + d.Customer.Givenname,
                Emailaddress = d.Customer.Emailaddress,
                Balance = d.Account.Balance,
            })
            .ToList();

            switch (countryId)

            {
                case "SE":
                    Flag = "/assets/img/flags/SE-flag-png-large.png";
                    break;
                case "FI":
                    Flag = "/assets/img/flags/FI-flag-png-large.png";
                    break;
                case "DK":
                    Flag = "/assets/img/flags/DK-flag-png-large.png";
                    break;
                case "NO":
                    Flag = "/assets/img/flags/NO-flag-png-large.png";
                    break;
            }

        }
    }

}
