using AutoMapper;
using BankWebApp.Services;
using BankWebApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankWebApp.Pages.Transactions
{
    [Authorize(Roles = "Cashier")]
    public class TransactionModel : PageModel
    {
        public const int BatchSize = 20;

        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public TransactionModel(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        public AccountViewModel Account { get; set; } = null!;
        public int CustomerId { get; set; }
        public List<TransactionViewModel> Transactions { get; set; } = new();
        public bool HasMore { get; set; }

        public IActionResult OnGet(int accountId, int customerId)
        {
            var account = _accountService.GetAccountForCustomer(accountId);
            if (account == null) return NotFound();

            Account = _mapper.Map<AccountViewModel>(account);
            CustomerId = customerId;

            var batch = _accountService.GetAccountTransactions(accountId, 0, BatchSize + 1);
            HasMore = batch.Count > BatchSize;
            Transactions = _mapper.Map<List<TransactionViewModel>>(batch.Take(BatchSize));
            return Page();
        }

        /// <summary>
        /// Called by the "Load 20 more" button: the next 20 transactions as table rows.
        /// The X-Has-More header says whether there are more after these.
        /// </summary>
        public IActionResult OnGetFetchMore(int accountId, int skip)
        {
            var batch = _accountService.GetAccountTransactions(accountId, skip, BatchSize + 1);
            Response.Headers["X-Has-More"] = batch.Count > BatchSize ? "true" : "false";
            return Partial("_TransactionRows", _mapper.Map<List<TransactionViewModel>>(batch.Take(BatchSize)));
        }
    }
}
