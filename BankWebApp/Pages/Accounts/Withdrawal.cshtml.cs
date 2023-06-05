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
    public class WithdrawalModel : PageModel
    {
        private readonly IAccountService _accountService;

        public WithdrawalViewModel Withdrawal { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter an amount.")]
        [Range(100, 10000, ErrorMessage = "Please choose a number between 100 and 10000.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public WithdrawalModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public class WithdrawalViewModel
        {
            public Account Account { get; set; }

            [Range(1, 1000000)]
            [DataType(DataType.Currency)]
            public decimal Amount { get; set; }

            public int CustomerId { get; set; }
        }

        public IActionResult OnGet(int accountId, int customerId)
        {
            Withdrawal = _accountService.CreateNewWithdrawalViewModel();
            Withdrawal.Account = _accountService.GetAccountForCustomer(accountId);
            Withdrawal.Amount = 0;
            Withdrawal.CustomerId = customerId;

            return Page();
        }

        public IActionResult OnPost(int accountId, int customerId)
        {
            var account = _accountService.GetAccountForCustomer(accountId);
            if (account.Balance < Amount)
            {
                ModelState.AddModelError("Amount", "Sorry, amount is too high!");
            }

            if (ModelState.IsValid)
            {
                _accountService.Withdraw(account, Amount);

                TempData["success"] = "Withdrawal made successfully!";

                return RedirectToPage("/Customers/CustomerCard", new { customerId = customerId });
            }

            Withdrawal = _accountService.CreateNewWithdrawalViewModel();
            Withdrawal.Account = _accountService.GetAccountForCustomer(accountId);
            Withdrawal.Amount = 0;
            Withdrawal.CustomerId = customerId;

            return Page();
        }
    }
}
