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
        public int Id { get; set; }
        public string SearchText { get; set; }
        public string SortOrder { get; set; }
        public string SortColumn { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int TotalPages { get; set; }

        public void OnGet(int accountId, string sortColumn, string sortOrder, string searchText, int pageNo = 1)
        {

            var allTransactions = _accountService.GetAccountTransactions(accountId)
                .OrderByDescending(t => t.Date)
                .ToList();


            // Pagination
            SearchText = searchText;
            SortOrder = sortOrder;
            SortColumn = sortColumn;
            int pageSize = 50;
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

        public IActionResult OnGetFetchMore(int accountId, long lastTransaction)
        {
            DateTime dateOfLastShown = new DateTime(lastTransaction).AddMilliseconds(100);

            var listOfTransactions = _accountService.GetAccountTransactions(accountId)
                .Where(t => lastTransaction == 0 || t.Date > dateOfLastShown)
                .OrderByDescending(t => t.Date)

                .Take(10)
                .ToList();

            var listOfTransactionsViewModel = _mapper.Map<List<TransactionViewModel>>(listOfTransactions);
            if (listOfTransactionsViewModel.Any())
                lastTransaction = listOfTransactionsViewModel.Last().Date.Ticks;
            return new JsonResult(new { items = listOfTransactionsViewModel, lastTransaction });
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
