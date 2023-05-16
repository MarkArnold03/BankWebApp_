using AutoMapper;
using BankWebApp.BankAppData;
using BankWebApp.ViewModel;
using BankWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace BankWebApp.Pages.Customers
{
    [Authorize(Roles = "Cashier")]
    public class CustomerCardModel : PageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public CustomerCardModel(ICustomerService customerService, IAccountService accountService, IMapper mapper)
        {
            _customerService = customerService;
            _accountService = accountService;
            _mapper = mapper;
        }

        public CustomerCardViewModel Customer { get; set; }
        public int CustmerId { get; set; }



        public void OnGet(int customerId)
        {
            CustmerId = customerId;
            var customer = _customerService.GetCustomer(customerId);
            Customer = _mapper.Map<CustomerCardViewModel>(customer);

            Customer.Name = $"{Customer.GivenName} {Customer.Surname}";
            Customer.Age = DateTime.Now.Year - Customer.Birthday.Year;
            Customer.Accounts = _accountService.GetAccountsForCustomer(customerId)
                .Select(a => new AccountViewModel { Id = a.AccountId, Balance = a.Balance })
                .ToList();
            Customer.TotalAccountValue = Customer.Accounts.Sum(a => a.Balance);
        }
    }


}

