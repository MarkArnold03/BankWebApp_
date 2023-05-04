namespace BankWebApp.Services
{
    public interface ICountryDataService
    {
        int GetCountryCustomersCount(string country);
        int GetCountryAccountsCount(string country);
        decimal GetCountryBalance(string country);
    }

}
