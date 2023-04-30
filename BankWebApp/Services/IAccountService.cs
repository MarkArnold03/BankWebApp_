using BankWebApp.BankAppData;
using BankWebApp.Infrastructure.Paging;
using Microsoft.AspNetCore.Mvc.Rendering;
using static BankWebApp.Pages.Accounts.AccountsModel;
using static BankWebApp.Pages.Accounts.DepositModel;
using static BankWebApp.Pages.Accounts.TransferModel;
using static BankWebApp.Pages.Accounts.WithdrawalModel;

namespace BankWebApp.Services
{
    public interface IAccountService
    {
        IEnumerable<Account> GetAllAccounts();
        PagedResult<Account> GetAccounts(string sortColumn, string sortOrder, string searchText, int page);
        IEnumerable<Account> GetAccountsForCustomer(int customerId);
        Account GetAccountForCustomer(int accountId);
        IEnumerable<Transaction> GetAccountTransactions(int accountId);
        Account CreateNewAccount();
        Disposition CreateNewDisposition();
        DepositViewModel CreateNewDepositViewModel();
        WithdrawalViewModel CreateNewWithdrawalViewModel();
        TransferViewModel CreateNewTransferViewModel();
        void Deposit(Account account, decimal amount);
        void Withdraw(Account account, decimal amount);
        void Transfer(Account fromAccount, Account toAccount, decimal amount);
        void AddAccount(int customerId, string frequency, decimal initialBalance);
    }

}
