using BankWebApp.BankAppData;
using BankWebApp.Infrastructure.Paging;
using Microsoft.EntityFrameworkCore;

namespace BankWebApp.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly BankAppDataContext _dbContext;

        public CustomerService(BankAppDataContext dbContext) => _dbContext = dbContext;

        public PagedResult<Customer> GetCustomers(int customerId, string sortColumn, string sortOrder, string searchText, int page)
        {
            var query = _dbContext.Customers.AsQueryable();

            switch (sortColumn)
            {
                case "CustomerId":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.CustomerId) : query.OrderByDescending(c => c.CustomerId);
                    break;
                case "NationalId":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.NationalId) : query.OrderByDescending(c => c.NationalId);
                    break;
                case "FullName":
                    query = sortOrder == "desc" ? query.OrderByDescending(c => c.Surname).ThenByDescending(c => c.Givenname) : query.OrderBy(c => c.Surname).ThenByDescending(c => c.Givenname);
                    break;
                case "Address":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.Streetaddress) : query.OrderByDescending(c => c.Streetaddress);
                    break;
                case "City":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.City) : query.OrderByDescending(c => c.City);
                    break;
                default:
                    // A stable order, so paging never repeats or skips customers.
                    query = query.OrderBy(c => c.CustomerId);
                    break;
            }

            // Every word must match the customer number, first name, surname or city,
            // so "anna uppsala" finds Anna in Uppsala.
            foreach (var word in (searchText ?? "").ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Where(c =>
                    c.CustomerId.ToString() == word ||
                    c.Givenname.ToLower().Contains(word) ||
                    c.Surname.ToLower().Contains(word) ||
                    c.City.ToLower().Contains(word));
            }

            return query.GetPaged(page, 50);
        }

        public IQueryable<Customer> GetCustomers() => _dbContext.Customers;

        public Customer? GetCustomer(int customerId) => _dbContext.Customers.Include(c => c.Dispositions).FirstOrDefault(c => c.CustomerId == customerId);

       
    }

}
