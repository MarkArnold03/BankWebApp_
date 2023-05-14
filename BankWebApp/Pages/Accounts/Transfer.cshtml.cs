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
    public class TransferModel : PageModel
    {
        private readonly IAccountService _accountService;

        public TransferViewModel Transfer { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter an amount.")]
        [Range(1, 1000000, ErrorMessage = "Please choose a number between 1 and 1000000.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please enter an account number.")]
        [BindProperty]
        [Range(1, 99999, ErrorMessage = "Please choose a valid account number.")]
        public int AccountToId { get; set; }

        public TransferModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public class TransferViewModel
        {
            public Account Account { get; set; }

            public int AccountToId { get; set; }

            public decimal Amount { get; set; }

            public int CustomerId { get; set; }
        }

        public IActionResult OnGet(int accountId, int customerId)
        {
            Transfer = _accountService.CreateNewTransferViewModel();
            Transfer.Account = _accountService.GetAccountForCustomer(accountId);
            Transfer.Amount = 0;
            Transfer.CustomerId = customerId;

            return Page();
        }

        public IActionResult OnPost(int accountId, int customerId)
        {
            var accountTo = _accountService.GetAccountForCustomer(AccountToId);
            var account = _accountService.GetAccountForCustomer(accountId);

            if (account.Balance < Amount)
            {
                ModelState.AddModelError("Amount", "Sorry, amount is greater than your current balance!");
            }

            if (accountTo == null)
            {
                ModelState.AddModelError("AccountToId", "Sorry, that account number does not exist!");
            }

            if (ModelState.IsValid)
            {
                _accountService.Transfer(account, accountTo, Amount);

                TempData["success"] = "Transfer made successfully!";

                return RedirectToPage("/Customers/CustomerCard", new { customerId = customerId });
            }

            Transfer = _accountService.CreateNewTransferViewModel();
            Transfer.Account = _accountService.GetAccountForCustomer(accountId);
            Transfer.Amount = 0;
            Transfer.CustomerId = customerId;

            return Page();
        }
    }
}
