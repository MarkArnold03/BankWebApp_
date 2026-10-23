namespace BankWebApp.ViewModel
{
    public class TransactionViewModel
    {
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = "";
        public string Operation { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string? Symbol { get; set; }
        public string? Bank { get; set; }
        public string? Account { get; set; }

        /// <summary>Debits are outgoing whether the import stored them as negative or positive amounts.</summary>
        public bool IsOutgoing => string.Equals(Type, "Debit", StringComparison.OrdinalIgnoreCase) || Amount < 0;
        public decimal SignedAmount => IsOutgoing ? -Math.Abs(Amount) : Math.Abs(Amount);
        public string Title => !string.IsNullOrWhiteSpace(Symbol) ? Symbol! : Operation;

        public string Details
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(Symbol)) parts.Add(Operation);
                if (!string.IsNullOrWhiteSpace(Bank)) parts.Add(Bank!);
                if (!string.IsNullOrWhiteSpace(Account)) parts.Add("Acct " + Account);
                return string.Join(" · ", parts);
            }
        }
    }
}
