using AutoMapper;
using BankWebApp.BankAppData;
using BankWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BankWebApp.Pages.Transactions
{
    public class TransactionModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public TransactionModel(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        public List<TransactionViewModel> Transactions { get; set; } = new List<TransactionViewModel>();
        public string SearchText { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public void OnGet(int accountId, string searchText, int pageNo = 1)
        {
            var allTransactions = _accountService.GetAccountTransactions(accountId)
                .OrderByDescending(t => t.Date)
                .ToList();

            // Filter by search text
            if (!string.IsNullOrEmpty(searchText))
            {
                allTransactions = allTransactions.Where(t => t.Amount.ToString().Contains(searchText)).ToList();
            }

            // Pagination
            int pageSize = 10;
            TotalPages = (int)Math.Ceiling(allTransactions.Count / (double)pageSize);

            if (pageNo < 1) pageNo = 1;
            else if (pageNo > TotalPages) pageNo = TotalPages;

            CurrentPage = pageNo;

            var transactionsForPage = allTransactions
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Transactions = _mapper.Map<List<TransactionViewModel>>(transactionsForPage);
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

        public class TransactionViewModel
        {
            public int TransactionId { get; set; }
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public decimal Balance { get; set; }
        }
    }
}
