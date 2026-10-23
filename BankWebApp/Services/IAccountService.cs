using BankWebApp.BankAppData;
using BankWebApp.Infrastructure.Paging;

namespace BankWebApp.Services
{
    public interface IAccountService
    {
        PagedResult<Account> GetAccounts(string sortColumn, string sortOrder, string searchText, int page);
        IEnumerable<Account> GetAccountsForCustomer(int customerId);

        /// <summary>The account with this number, or null if there is none.</summary>
        Account? GetAccountForCustomer(int accountId);

        /// <summary>Transactions newest first, for paging ("load more").</summary>
        IReadOnlyList<Transaction> GetAccountTransactions(int accountId, int skip, int take);

        void Deposit(Account account, decimal amount);
        void Withdraw(Account account, decimal amount);
        void Transfer(Account fromAccount, Account toAccount, decimal amount);
    }
}
