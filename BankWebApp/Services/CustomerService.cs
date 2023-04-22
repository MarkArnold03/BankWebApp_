using BankWebApp.BankAppData;
using BankWebApp.Infrastructure.Paging;
using Microsoft.EntityFrameworkCore;

namespace BankWebApp.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly BankAppDataContext _dbContext;

        public CustomerService(BankAppDataContext dbContext)
        {
            _dbContext = dbContext;
        }
        public PagedResult<Customer> GetCustomers(int customerId, string sortColumn, string sortOrder, string searchText, int page)
        {
            var query = _dbContext.Customers.AsQueryable();

            if (sortColumn == "CustomerId" || string.IsNullOrEmpty(sortColumn))
                if (sortOrder == "asc")
                    query = query.OrderBy(c => c.CustomerId);
                else
                    query = query.OrderByDescending(c => c.CustomerId);

            if (sortColumn == "NationalId" || string.IsNullOrEmpty(sortColumn))
                if (sortOrder == "asc")
                    query = query.OrderBy(c => c.NationalId);
                else
                    query = query.OrderByDescending(c => c.NationalId);

            else if (sortColumn == "FullName" || string.IsNullOrEmpty(sortColumn))
                if (sortOrder == "desc")
                    query = query.OrderByDescending(c => c.Surname).ThenByDescending(c => c.Givenname);
                else
                    query = query.OrderBy(c => c.Surname).ThenByDescending(c => c.Givenname);


            else if (sortColumn == "Address" || string.IsNullOrEmpty(sortColumn))
                if (sortOrder == "asc")
                    query = query.OrderBy(c => c.Streetaddress);
                else
                    query = query.OrderByDescending(c => c.Streetaddress);

            else if (sortColumn == "City" || string.IsNullOrEmpty(sortColumn))
                if (sortOrder == "asc")
                    query = query.OrderBy(c => c.City);
                else
                    query = query.OrderByDescending(c => c.City);

            //Om det finns någon söksträng...
            if (searchText != null)
            {
                query = query
                .Where(c =>
                c.CustomerId.ToString() == searchText ||
                c.Givenname.ToLower().Contains(searchText.ToLower()) ||
                c.Surname.ToLower().Contains(searchText.ToLower()) ||
                c.City.ToLower().Contains(searchText.ToLower())
                );
            }

            return query.GetPaged(page, 50);
        }
        public IEnumerable<Customer> GetCustomers()
        {
            var customers = _dbContext.Customers;
            return customers;
        }

        public Customer GetCustomer(int customerId)
        {
            var customer = _dbContext.Customers
                .Include(c => c.Dispositions)
                .First(c => c.CustomerId == customerId);

            return customer;
        }
    }
}
