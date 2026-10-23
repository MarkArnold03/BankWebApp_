using BankWebApp.BankAppData;
using BankWebApp.Infrastructure.Paging;

namespace BankWebApp.Services
{
    public interface ICustomerService
    {
        PagedResult<Customer> GetCustomers(int customerId, string sortColumn, string sortOrder, string searchText, int page);
        IQueryable<Customer> GetCustomers();

        /// <summary>The customer with this number, or null if there is none.</summary>
        Customer? GetCustomer(int customerId);
    }
}
