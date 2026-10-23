using BankWebApp.Services;
using BankWebApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankWebApp.Pages.Countries
{
    // Shows customers' names, emails and balances, so only staff may see it,
    // and it may only be cached in the staff member's own browser.
    [Authorize(Roles = "Cashier,Admin")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, VaryByQueryKeys = new[] { "countryId" })]
    public class TopTenModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public TopTenModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public IEnumerable<CountryViewModel> Top10Customers { get; set; } = Enumerable.Empty<CountryViewModel>();
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";

        public IActionResult OnGet(string countryId)
        {
            var country = Pages.IndexModel.Countries.FirstOrDefault(c => c.Code == countryId);
            if (country.Code == null) return NotFound();

            CountryCode = country.Code;
            CountryName = country.Name;

            // Owners only, so a shared account isn't listed twice.
            Top10Customers = _customerService.GetCustomers()
                .Where(c => c.CountryCode == countryId)
                .SelectMany(c => c.Dispositions.Where(d => d.Type == "OWNER"))
                .OrderByDescending(d => d.Account.Balance)
                .ThenBy(d => d.AccountId)
                .Take(10)
                .Select(d => new CountryViewModel
                {
                    Id = d.CustomerId,
                    AccountId = d.AccountId,
                    Name = d.Customer.Givenname + " " + d.Customer.Surname,
                    Emailaddress = d.Customer.Emailaddress,
                    Balance = d.Account.Balance,
                })
                .ToList();

            return Page();
        }
    }
}
