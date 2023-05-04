using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BankWebApp.Services;

namespace BankWebApp.Pages
{
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

        public async Task<IActionResult> OnGet()
        {
            var countries = new[] { "SE", "FI", "DK", "NO" };

            foreach (var country in countries)
            {
                var viewModel = new IndexViewModel
                {
                    NumberOfClients = await _countryDataService.GetCountryCustomersCount(country),
                    NumberOfAccounts = await _countryDataService.GetCountryAccountsCount(country),
                    TotalAccountValue = await _countryDataService.GetCountryBalance(country),
                };

                ViewModels[country] = viewModel;
            }

            return Page();
        }
    }
}