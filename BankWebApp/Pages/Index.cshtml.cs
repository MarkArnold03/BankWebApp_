using BankWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BankWebApp.Services;

namespace BankWebApp.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICountryDataService _countryDataService;

        public Dictionary<string, IndexViewModel> ViewModels { get; set; } = new Dictionary<string, IndexViewModel>();

        public class IndexViewModel
        {
            public int NumberOfClients { get; set; }
            public int NumberOfAccounts { get; set; }
            public decimal TotalAccountValue { get; set; }
        }

        public IndexModel(
            ILogger<IndexModel> logger,
            ICountryDataService countryDataService)
        {
            _logger = logger;
            _countryDataService = countryDataService;
        }

        public IActionResult OnGet()
        {
            var countries = new[] { "SE", "FI", "DK", "NO" };

            foreach (var country in countries)
            {
                var viewModel = new IndexViewModel
                {
                    NumberOfClients = _countryDataService.GetCountryCustomersCount(country),
                    NumberOfAccounts = _countryDataService.GetCountryAccountsCount(country),
                    TotalAccountValue = _countryDataService.GetCountryBalance(country),
                };

                ViewModels[country] = viewModel;
            }

            return Page();
        }
    }
}