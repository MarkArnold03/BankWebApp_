namespace BankWebApp.ViewModel
{
    public class CustomerCardViewModel
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public DateTime Birthday { get; set; }
        public int Age { get; set; }
        public string NationalId { get; set; }
        public string TelephoneCountryCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public List<AccountViewModel> Accounts { get; set; }
        public decimal TotalAccountValue { get; set; }
    }
}
