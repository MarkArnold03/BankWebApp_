using AutoMapper;
using BankWebApp.Services;
using BankWebApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public CustomerCardViewModel Customer { get; set; } = null!;
        public int CustmerId { get; set; }

        public IActionResult OnGet(int customerId)
        {
            var customer = _customerService.GetCustomer(customerId);
            if (customer == null) return NotFound();

            CustmerId = customerId;
            Customer = _mapper.Map<CustomerCardViewModel>(customer);
            Customer.Age = Customer.Birthday == null ? null : AgeOn(Customer.Birthday.Value, DateTime.Today);
            Customer.Accounts = _mapper.Map<List<AccountViewModel>>(_accountService.GetAccountsForCustomer(customerId));
            Customer.TotalAccountValue = Customer.Accounts.Sum(a => a.Balance);
            return Page();
        }

        /// <summary>Whole years, counting the birthday only once it has passed this year.</summary>
        public static int AgeOn(DateTime birthday, DateTime today)
        {
            var age = today.Year - birthday.Year;
            if (birthday.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
