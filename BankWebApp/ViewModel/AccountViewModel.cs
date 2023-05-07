using System.ComponentModel.DataAnnotations;

namespace BankWebApp.ViewModel
{
    public class AccountViewModel
    {
        public int Id { get; set; }

        [MaxLength(70)]
        public string Frequency { get; set; } = null!;

        public DateTime Created { get; set; }

        [MaxLength(70)]
        public decimal Balance { get; set; }

    }
}
