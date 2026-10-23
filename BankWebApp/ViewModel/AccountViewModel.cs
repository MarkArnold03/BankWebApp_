namespace BankWebApp.ViewModel
{
    public class AccountViewModel
    {
        public int CustomerId { get; set; }
        public int Id { get; set; }
        public string Frequency { get; set; } = "";
        public DateTime Created { get; set; }
        public decimal Balance { get; set; }
    }
}
