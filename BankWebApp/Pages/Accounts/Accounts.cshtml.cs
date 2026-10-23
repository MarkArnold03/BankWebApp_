using AutoMapper;
using BankWebApp.Services;
using BankWebApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankWebApp.Pages.Accounts
{
    [Authorize(Roles = "Cashier")]
    public class AccountsModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        public List<AccountViewModel> Accounts { get; set; } = new();

        public string? SortOrder { get; set; }
        public string? SortColumn { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
        public string? SearchText { get; set; }

        public AccountsModel(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        public IActionResult OnGet(string? sortColumn, string? sortOrder, string? searchText, int pageNo)
        {
            SearchText = searchText?.Trim();

            // An account number on its own opens that account directly.
            if (int.TryParse(SearchText, out var accountId) && _accountService.GetAccountForCustomer(accountId) != null)
                return RedirectToPage("/Accounts/Transactions", new { accountId });

            SortOrder = sortOrder == "desc" ? "desc" : "asc";
            SortColumn = sortColumn;
            CurrentPage = Math.Max(1, pageNo);

            var pageresult = _accountService.GetAccounts(sortColumn ?? "", SortOrder, SearchText ?? "", CurrentPage);
            PageCount = Math.Max(1, pageresult.PageCount);
            RowCount = pageresult.RowCount;
            Accounts = _mapper.Map<List<AccountViewModel>>(pageresult.Results);
            return Page();
        }
    }
}
