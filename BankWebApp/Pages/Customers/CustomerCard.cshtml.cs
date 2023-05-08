//using BankWebApp.Services;
//using BankWebApp.ViewModel;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using System.Data;

//namespace BankWebApp.Pages.Customers
//{
//    [Authorize(Roles = "Cashier")]
//    public class CustomerModel : PageModel
//    {
//        private readonly ICustomerService _customerService;
//        private readonly IAccountService _accountService;

//        public CustomerModel(ICustomerService customerService, IAccountService accountService)
//        {
//            _customerService = customerService;
//            _accountService = accountService;
//        }

//        public CustomersViewModel Customer { get; set; } = new CustomersViewModel();

//        public class AccountViewModel
//        {
//            public int Id { get; set; }
//            public string Type { get; set; } = null!;
//            public decimal Balance { get; set; }
//        }

//        public IActionResult OnGet(int customerId)
//        {
//            var customer = _customerService.GetCustomer(customerId);

//            if (customer == null)
//            {
//                return NotFound();
//            }

//            var customerAccounts = _accountService.GetAccountsForCustomer(customerId);
//            var accountViewModels = customerAccounts.Select(a => new AccountViewModel
//            {
//                Id = a.Id,
//                Type = a.Type,
//                Balance = a.Balance
//            }).ToList();

//            var viewModel = new CustomersViewModel
//            {
//                Id = customerId,
//                Name = $"{customer.Givenname} {customer.Surname}",
//                Address = $"{customer.Streetaddress}, {customer.Zipcode} {customer.City}, {customer.Country}",
//                ContactInfo = $"Email: {customer.Emailaddress}, Phone: {customer.Telephonenumber}",
//                Accounts = accountViewModels,
//                TotalAccountValue = Math.Round(accountViewModels.Sum(a => a.Balance), 2)
//            };

//            Customer = viewModel;

//            return Page();
//        }
//    }

//}
