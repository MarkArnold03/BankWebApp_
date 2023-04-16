using BankWebApp.BankAppData;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BankWebApp.Services
{
    public interface ICustomerService
    {
        List<Customer> GetCustomers(int customerId, string sortColumn, string sortOrder, string searchText, int page);
        IEnumerable<Customer> GetCustomers();
        Customer GetCustomer(int customerId);
        BankAppDataContext GetDbContext();
        List<SelectListItem> FillGenderDropDown();
        List<SelectListItem> FillCountryDropDown();
        Customer GetNewCustomer();
        void CreateCustomer(Customer customer);
        void EditCustomer();
        void Update();
    }
}
