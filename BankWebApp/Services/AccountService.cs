using BankWebApp.BankAppData;
using BankWebApp.Services;
using BankWebApp.Infrastructure.Paging;
using static BankWebApp.Pages.Accounts.DepositModel;
using static BankWebApp.Pages.Accounts.TransferModel;
using static BankWebApp.Pages.Accounts.WithdrawalModel;
using Microsoft.EntityFrameworkCore;

namespace BankWebApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankAppDataContext _context;

        public AccountService(BankAppDataContext context)
        {
            _context = context;
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            return _context.Accounts;
        }

        public PagedResult<Account> GetAccounts(string sortColumn, string sortOrder, string searchText, int page)
        {
            var query = _context.Accounts.AsQueryable();

            if (string.IsNullOrEmpty(sortColumn) || sortColumn == "AccountId")
                query = sortOrder == "asc" ? query.OrderBy(a => a.AccountId) : query.OrderByDescending(a => a.AccountId);

            if (string.IsNullOrEmpty(sortColumn) || sortColumn == "Frequency")
                query = sortOrder == "asc" ? query.OrderBy(a => a.Frequency) : query.OrderByDescending(a => a.Frequency);

            if (string.IsNullOrEmpty(sortColumn) || sortColumn == "Created")
                query = sortOrder == "desc" ? query.OrderByDescending(a => a.Created) : query.OrderBy(a => a.Created);

            if (string.IsNullOrEmpty(sortColumn) || sortColumn == "Balance")
                query = sortOrder == "asc" ? query.OrderBy(a => a.Balance) : query.OrderByDescending(a => a.Balance);

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query
                .Where(a =>
                a.AccountId.ToString() == searchText ||
                a.Frequency.ToLower().Contains(searchText.ToLower()) ||
                a.Created.ToString() == searchText ||
                a.Balance.ToString() == searchText
                );
            }

            return query.GetPaged(page, 50);
        }

        public IEnumerable<Account> GetAccountsForCustomer(int customerId)
        {
            return _context.Dispositions
                .Where(d => d.CustomerId == customerId)
                .Select(d => d.Account);
        }

        public Account GetAccountForCustomer(int accountId)
        {
            return _context.Accounts
                .FirstOrDefault(a => a.AccountId == accountId);
        }

        public IEnumerable<Transaction> GetAccountTransactions(int accountId)
        {
            var account = _context.Accounts
                .Include(a => a.Transactions)
                .First(a => a.AccountId == accountId);

            return account.Transactions.ToList();
        }

        public Account CreateNewAccount()
        {
            return new Account();
        }

        public Disposition CreateNewDisposition()
        {
            return new Disposition();
        }

        public DepositViewModel CreateNewDepositViewModel()
        {
            return new DepositViewModel();
        }

        public WithdrawalViewModel CreateNewWithdrawalViewModel()
        {
            return new WithdrawalViewModel();
        }

        public TransferViewModel CreateNewTransferViewModel()
        {
            return new TransferViewModel();
        }

        public void Deposit(Account account, decimal amount)
        {
            account.Balance += amount;
            account.Transactions.Add(new Transaction
            {
                AccountId = account.AccountId,
                Date = DateTime.Now,
                Type = "Debit",
                Operation = "Withdrawal in Cash",
                Amount = amount,
                Balance = account.Balance,
            });

            _context.SaveChanges();
        }

        public void Withdraw(Account account, decimal amount)
        {
            account.Balance -= amount;
            account.Transactions.Add(new Transaction
            {
                AccountId = account.AccountId,
                Date = DateTime.Now,
                Type = "Debit",
                Operation = "Withdrawal in Cash",
                Amount = amount * -1,
                Balance = account.Balance,
            });

            _context.SaveChanges();
        }

        public void Transfer(Account fromAccount, Account toAccount, decimal amount)
        {
            fromAccount.Balance -= amount;
            fromAccount.Transactions.Add(new Transaction
            {
                AccountId = fromAccount.AccountId,
                Date = DateTime.Now,

                Type = "Debit",
                Operation = "Remittance to Another Bank",
                Amount = amount * -1,
                Balance = fromAccount.Balance,
            });

            toAccount.Balance += amount;
            toAccount.Transactions.Add(new Transaction
            {
                AccountId = toAccount.AccountId,
                Date = DateTime.Now,
                Type = "Credit",
                Operation = "Collection from Another Bank",
                Amount = amount,
                Balance = toAccount.Balance,
            });

            _context.SaveChanges();
        }

        public void AddAccount(int customerId, string frequency, decimal initialBalance)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            var account = CreateNewAccount();
            var disposition = CreateNewDisposition();

            disposition.CustomerId = customer.CustomerId;
            disposition.Account = account;
            disposition.AccountId = account.AccountId;
            disposition.Type = "OWNER";

            account.Frequency = frequency;

            Deposit(account, initialBalance);

            account.Created = DateTime.Now;

            customer.Dispositions.Add(disposition);

            _context.SaveChanges();
        }

    }

}
 
