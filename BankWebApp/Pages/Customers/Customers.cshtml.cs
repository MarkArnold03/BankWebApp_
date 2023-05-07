using AutoMapper;
using BankWebApp.BankAppData;
using BankWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using BankWebApp.ViewModel;



namespace BankWebApp.Pages.Customers
{
    [Authorize(Roles = "Cashier")]
    public class CustomersModel : PageModel
    {
        private readonly ICustomerService _customerContext;
        private readonly IMapper _mapper;
        public List<CustomersViewModel> Customers { get; set; }

        public CustomersModel(ICustomerService customerContext, IMapper mapper)
        {
            _customerContext = customerContext;
            _mapper = mapper;
        }

        public int Id { get; set; }
        public string SortOrder { get; set; }
        public string SortColumn { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public string SearchText { get; set; }

       

        
        


        public void OnGet(int customerId, string sortColumn, string sortOrder, string searchText, int pageNo)
        {
            SearchText = searchText;
            SortOrder = sortOrder;
            SortColumn = sortColumn;
            if (pageNo == 0)
                pageNo = 1;
            CurrentPage = pageNo;
            Id = customerId; 
            
            var pageresult = _customerContext.GetCustomers(customerId, sortColumn, sortOrder, searchText, CurrentPage);
            PageCount = pageresult.PageCount;

            Customers = _mapper.Map<List<CustomersViewModel>>(pageresult.Results);




        }
    }
}
