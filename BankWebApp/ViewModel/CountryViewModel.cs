namespace BankWebApp.ViewModel
{
    public class CountryViewModel
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Emailaddress { get; set; }
        public decimal Balance { get; set; }
    }
}
