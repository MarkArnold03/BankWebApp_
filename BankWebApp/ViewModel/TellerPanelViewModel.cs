namespace BankWebApp.ViewModel
{
    /// <summary>The deposit / withdraw / transfer panel shown beside an account's ledger.</summary>
    public class TellerPanelViewModel
    {
        public AccountViewModel Account { get; set; } = null!;
        public int CustomerId { get; set; }

        /// <summary>"deposit", "withdraw" or "transfer": the tab that is open.</summary>
        public string Active { get; set; } = "deposit";

        // Values and messages to show again after a failed attempt.
        public string? Amount { get; set; }
        public string? AccountTo { get; set; }
        public string? AmountError { get; set; }
        public string? AccountToError { get; set; }
    }
}
