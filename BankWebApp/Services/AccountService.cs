using BankWebApp.BankAppData;
using BankWebApp.Infrastructure.Paging;
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

        public PagedResult<Account> GetAccounts(string sortColumn, string sortOrder, string searchText, int page)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                var text = searchText.Trim().ToLower();
                query = query.Where(a =>
                    a.AccountId.ToString() == text ||
                    a.Frequency.ToLower().Contains(text));
            }

            var ascending = sortOrder != "desc";
            query = sortColumn switch
            {
                "Frequency" => ascending ? query.OrderBy(a => a.Frequency).ThenBy(a => a.AccountId) : query.OrderByDescending(a => a.Frequency).ThenBy(a => a.AccountId),
                "Created" => ascending ? query.OrderBy(a => a.Created).ThenBy(a => a.AccountId) : query.OrderByDescending(a => a.Created).ThenBy(a => a.AccountId),
                "Balance" => ascending ? query.OrderBy(a => a.Balance).ThenBy(a => a.AccountId) : query.OrderByDescending(a => a.Balance).ThenBy(a => a.AccountId),
                _ => ascending ? query.OrderBy(a => a.AccountId) : query.OrderByDescending(a => a.AccountId),
            };

            return query.GetPaged(page, 50);
        }

        public IEnumerable<Account> GetAccountsForCustomer(int customerId)
        {
            return _context.Dispositions
                .Where(d => d.CustomerId == customerId)
                .OrderBy(d => d.AccountId)
                .Select(d => d.Account)
                .ToList();
        }

        public Account? GetAccountForCustomer(int accountId)
        {
            return _context.Accounts
                .FirstOrDefault(a => a.AccountId == accountId);
        }

        public IReadOnlyList<Transaction> GetAccountTransactions(int accountId, int skip, int take)
        {
            return _context.Transactions
                .AsNoTracking()
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.TransactionId)
                .Skip(Math.Max(0, skip))
                .Take(Math.Clamp(take, 1, 100))
                .ToList();
        }

        public void Deposit(Account account, decimal amount)
        {
            EnsureValidAmount(amount);
            Book(account, amount, "Credit", "Credit in Cash");
            _context.SaveChanges();
        }

        public void Withdraw(Account account, decimal amount)
        {
            EnsureValidAmount(amount);
            EnsureFunds(account, amount);
            Book(account, -amount, "Debit", "Withdrawal in Cash");
            _context.SaveChanges();
        }

        public void Transfer(Account fromAccount, Account toAccount, decimal amount)
        {
            EnsureValidAmount(amount);
            if (fromAccount.AccountId == toAccount.AccountId)
                throw new InvalidOperationException("Can't transfer to the same account.");
            EnsureFunds(fromAccount, amount);

            Book(fromAccount, -amount, "Debit", "Remittance to Another Bank", toAccount.AccountId);
            Book(toAccount, amount, "Credit", "Collection from Another Bank", fromAccount.AccountId);

            // One SaveChanges call: both balances and both transactions are saved together or not at all.
            _context.SaveChanges();
        }

        /// <summary>
        /// The only place a balance changes: every change is written as a transaction
        /// that carries the new balance.
        /// </summary>
        private static void Book(Account account, decimal signedAmount, string type, string operation, int? counterAccount = null)
        {
            account.Balance += signedAmount;
            account.Transactions.Add(new Transaction
            {
                AccountId = account.AccountId,
                Date = DateTime.Now,
                Type = type,
                Operation = operation,
                Amount = signedAmount,
                Balance = account.Balance,
                Account = counterAccount?.ToString(),
            });
        }

        private static void EnsureValidAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be greater than zero.");
            if (decimal.Round(amount, 2) != amount)
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount can have at most two decimals.");
        }

        private static void EnsureFunds(Account account, decimal amount)
        {
            if (account.Balance < amount)
                throw new InvalidOperationException("Insufficient funds.");
        }
    }
}
