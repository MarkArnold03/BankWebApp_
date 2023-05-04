using BankWebApp.BankAppData;
using BankWebApp.Infrastructure.Paging;

namespace BankWebApp.Services
{
    public interface ICustomerService
    {
        PagedResult<Customer> GetCustomers(int customerId, string sortColumn, string sortOrder, string searchText, int page);
        IEnumerable<Customer> GetCustomers();
        Customer GetCustomer(int customerId);
       
    }

}
