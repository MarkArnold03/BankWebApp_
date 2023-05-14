using System.ComponentModel.DataAnnotations;
using BankWebApp.BankAppData;

namespace BankWebApp.ViewModel
{
    public class AccountViewModel
    {
        public int CustomerId { get; set; }
        public int Id { get; set; }

        [MaxLength(70)]
        public string Frequency { get; set; } = null!;

        public DateTime Created { get; set; }

        [MaxLength(70)]
        public decimal Balance { get; set; }

        public ICollection<Transaction> Transactions { get; set; }

    }
}
