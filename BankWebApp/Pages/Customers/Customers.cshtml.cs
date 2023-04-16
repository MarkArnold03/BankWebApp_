using BankWebApp.BankAppData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Pages.Customers
{
    public class CustomersModel : PageModel
    {
        private readonly BankAppDataContext _dbContext;

        public CustomersModel(BankAppDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public class CustomersViewModel
        {
            public int Id { get; set; }
            [MaxLength(12)]
            public string NationalId { get; set; } = null!;
            [MaxLength(70)]
            public string Givenname { get; set; } = null!;
            [MaxLength(70)]
            public string Surname { get; set; } = null!;
            [MaxLength(140)]
            public string FullName { get; set; } = null!;
            [MaxLength(100)]
            public string StreetAddress { get; set; } = null!;
            [MaxLength(100)]
            public string City { get; set; } = null!;
        }

        public List<CustomersViewModel> Customers { get; set; } = new List<CustomersViewModel>();
        public void OnGet()
        {
            //Customers = _dbContext.Customers.Select(c => new CustomersViewModel
            //{
            //    Id = s.SupplierId,
            //    CompanyName = s.CompanyName,
            //    Region = s.Region
            //}).ToList();

        }
    }
}
