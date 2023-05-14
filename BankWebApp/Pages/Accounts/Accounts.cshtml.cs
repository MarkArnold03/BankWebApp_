using AutoMapper;
using BankWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using BankWebApp.ViewModel;

namespace BankWebApp.Pages.Accounts
{
    [Authorize(Roles = "Cashier")]
    public class AccountsModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        public List<AccountViewModel> Accounts { get; set; }

        public int Id { get; set; }
        public string SortOrder { get; set; }
        public string SortColumn { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public string SearchText { get; set; }



        public AccountsModel(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }


        

        public class TransactionViewModel
        {
            public int TransactionId { get; set; }
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public decimal Balance { get; set; }

        }

         public IActionResult OnGetFetchMore(int accountId, int lastTransactionId)
        {
            var moreTransactions = _accountService.GetAccountTransactions(accountId)
                .Where(t => t.TransactionId < lastTransactionId)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToList();

            var moreTransactionsViewModel = _mapper.Map<List<TransactionViewModel>>(moreTransactions);

            return new JsonResult(moreTransactionsViewModel);
        }

        public void OnGet(string sortColumn, string sortOrder, string searchText, int pageNo)
        {
            SearchText = searchText;
            SortOrder = sortOrder;
            SortColumn = sortColumn;
            if (pageNo == 0)
                pageNo = 1;
            CurrentPage = pageNo;

            var pageresult = _accountService.GetAccounts(sortColumn, sortOrder, searchText, CurrentPage);
            PageCount = pageresult.PageCount;

            Accounts = _mapper.Map<List<AccountViewModel>>(pageresult.Results);

        }

       

    }
}
