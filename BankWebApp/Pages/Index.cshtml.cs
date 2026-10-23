using BankWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankWebApp.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        public static readonly (string Code, string Name)[] Countries =
        {
            ("SE", "Sweden"), ("NO", "Norway"), ("DK", "Denmark"), ("FI", "Finland")
        };

        private readonly ILogger<IndexModel> _logger;
        private readonly ICountryDataService _countryDataService;

        public Dictionary<string, IndexViewModel> ViewModels { get; set; } = new Dictionary<string, IndexViewModel>();
        public IndexViewModel Totals { get; set; } = new IndexViewModel();

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
            var totals = _countryDataService.GetBankTotals();
            Totals = new IndexViewModel
            {
                NumberOfClients = totals.Customers,
                NumberOfAccounts = totals.Accounts,
                TotalAccountValue = totals.TotalBalance,
            };

            foreach (var (country, _) in Countries)
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
