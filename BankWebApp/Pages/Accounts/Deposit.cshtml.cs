using BankWebApp.BankAppData;
using BankWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace BankWebApp.Pages.Accounts
{
    [Authorize(Roles = "Cashier")]
    public class DepositModel : PageModel
    {
        private readonly IAccountService _accountService;

        public DepositViewModel Deposit { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter an amount.")]
        [Range(1, 1000000, ErrorMessage = "Please choose a number between 1 and 1000000.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public DepositModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public class DepositViewModel
        {
            public Account Account { get; set; }

            [Range(1, 1000000)]
            public decimal Amount { get; set; }

            public int CustomerId { get; set; }
        }

        public IActionResult OnGet(int accountId, int customerId)
        {
            Deposit = _accountService.CreateNewDepositViewModel();
            Deposit.Account = _accountService.GetAccountForCustomer(accountId);
            Deposit.Amount = 0;
            Deposit.CustomerId = customerId;

            return Page();
        }

        public IActionResult OnPost(int accountId, int customerId)
        {
            var account = _accountService.GetAccountForCustomer(accountId);

            if (ModelState.IsValid)
            {
                _accountService.Deposit(account, Amount);

                TempData["success"] = "Deposit made successfully!";

                return RedirectToPage("/Customers/Customer", new { customerId = customerId });
            }

            Deposit = _accountService.CreateNewDepositViewModel();
            Deposit.Account = _accountService.GetAccountForCustomer(accountId);
            Deposit.Amount = 0;
            Deposit.CustomerId = customerId;

            return Page();
        }
    }
}
