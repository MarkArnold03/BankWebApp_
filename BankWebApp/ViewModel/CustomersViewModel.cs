using System.ComponentModel.DataAnnotations;

namespace BankWebApp.ViewModel
{
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
}
