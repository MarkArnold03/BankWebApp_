using AutoMapper;
using BankWebApp.Services;
using BankWebApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankWebApp.Pages.Customers
{
    [Authorize(Roles = "Cashier")]
    public class CustomersModel : PageModel
    {
        private readonly ICustomerService _customerContext;
        private readonly IMapper _mapper;
        public List<CustomersViewModel> Customers { get; set; } = new();

        public CustomersModel(ICustomerService customerContext, IMapper mapper)
        {
            _customerContext = customerContext;
            _mapper = mapper;
        }

        public string? SortOrder { get; set; }
        public string? SortColumn { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
        public string? SearchText { get; set; }

        public IActionResult OnGet(string? sortColumn, string? sortOrder, string? searchText, int pageNo)
        {
            SearchText = searchText?.Trim();

            // A customer number on its own opens that customer's profile directly.
            if (int.TryParse(SearchText, out var customerId) && _customerContext.GetCustomer(customerId) != null)
                return RedirectToPage("/Customers/CustomerCard", new { customerId });

            SortOrder = sortOrder == "desc" ? "desc" : "asc";
            SortColumn = sortColumn;
            CurrentPage = Math.Max(1, pageNo);

            var pageresult = _customerContext.GetCustomers(0, sortColumn ?? "", SortOrder, SearchText ?? "", CurrentPage);
            PageCount = Math.Max(1, pageresult.PageCount);
            RowCount = pageresult.RowCount;

            if (CurrentPage > PageCount)
            {
                CurrentPage = PageCount;
                pageresult = _customerContext.GetCustomers(0, sortColumn ?? "", SortOrder, SearchText ?? "", CurrentPage);
            }

            Customers = _mapper.Map<List<CustomersViewModel>>(pageresult.Results);
            return Page();
        }
    }
}
