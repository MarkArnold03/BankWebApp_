using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace BankWebApp.Data
{
    public class DataInitializer
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public DataInitializer(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        public void SeedData()
        {
            // Migrations target SQL Server; other providers (the tests use SQLite) create the schema themselves.
            if (_dbContext.Database.IsSqlServer())
                _dbContext.Database.Migrate();
            SeedRoles();
            SeedUsers();
        }

        
        private void SeedUsers()
        {
            AddUserIfNotExists("richard.chalk@systementor.se", "Hejsan123#", new string[] { "Admin" });
            AddUserIfNotExists("richard.erdos.chalk@gmail.se", "Hejsan123#", new string[] { "Cashier" });
        }

       
        private void SeedRoles()
        {
            AddRoleIfNotExisting("Admin");
            AddRoleIfNotExisting("Cashier");
        }

        private void AddRoleIfNotExisting(string roleName)
        {
            // Identity looks roles up by their upper-case normalized name ("ADMIN").
            var normalized = roleName.ToUpperInvariant();
            var role = _dbContext.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                _dbContext.Roles.Add(new IdentityRole { Name = roleName, NormalizedName = normalized });
                _dbContext.SaveChanges();
            }
            else if (role.NormalizedName != normalized)
            {
                role.NormalizedName = normalized;
                _dbContext.SaveChanges();
            }
        }

        private void AddUserIfNotExists(string userName, string password, string[] roles)
        {
            if (_userManager.FindByEmailAsync(userName).Result != null) return;

            var user = new IdentityUser
            {
                UserName = userName,
                Email = userName,
                EmailConfirmed = true
            };
            _userManager.CreateAsync(user, password).Wait();
            _userManager.AddToRolesAsync(user, roles).Wait();
        }

        //private void SeedCountries()
        //{
        //    CountryDoesNotExist("SE", "Sweden");
        //    CountryDoesNotExist("NO", "Norway");
        //    CountryDoesNotExist("DK", "Denmark");
        //    CountryDoesNotExist("FI", "Finland");
        //}

        //private void CountryDoesNotExist(string countryCode, string countryLabel)
        //{
        //    // Check to see if country already exists in db
        //    if (_dbContext.Countries.Any(c => c.CountryCode == countryCode)) return;
        //    _dbContext.Countries
        //        .Add(new Country
        //        {
        //            CountryCode = countryCode,
        //            CountryLabel = countryLabel
        //        });
        //    _dbContext.SaveChanges();
        //}
    }
}
