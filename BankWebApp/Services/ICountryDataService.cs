namespace BankWebApp.Services
{
    public interface ICountryDataService
    {
        int GetCountryCustomersCount(string country);
        int GetCountryAccountsCount(string country);
        decimal GetCountryBalance(string country);

        /// <summary>Customers, accounts and the summed balance of every account in the bank.</summary>
        (int Customers, int Accounts, decimal TotalBalance) GetBankTotals();
    }
}
