using System.Globalization;

namespace BankWebApp.Helpers
{
    /// <summary>Swedish-style money and number formatting, e.g. "48 312,40 kr".</summary>
    public static class BankFormat
    {
        private static readonly CultureInfo Sv = CultureInfo.GetCultureInfo("sv-SE");

        public static string Sek(decimal amount) =>
            (amount < 0 ? "−" : "") + Math.Abs(amount).ToString("#,##0.00", Sv) + " kr";

        public static string SignedSek(decimal amount) =>
            (amount > 0 ? "+" : amount < 0 ? "−" : "") + Math.Abs(amount).ToString("#,##0.00", Sv) + " kr";

        public static string AccountNumber(int accountId) => accountId.ToString("#,##0", Sv);

        public static string ShortDate(DateTime date) =>
            date.Year == DateTime.Today.Year ? date.ToString("d MMM", CultureInfo.InvariantCulture) : date.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

        public static string DayHeading(DateTime date)
        {
            if (date.Date == DateTime.Today) return "Today";
            if (date.Date == DateTime.Today.AddDays(-1)) return "Yesterday";
            return date.ToString(date.Year == DateTime.Today.Year ? "dddd d MMMM" : "d MMMM yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>Reads amounts the way people type them: "1 250,50", "1250.50" or "1 250,50 kr".</summary>
        public static bool TryParseAmount(string? text, out decimal amount)
        {
            amount = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            var cleaned = new string(text.Where(c => !char.IsWhiteSpace(c) && c != '\u00a0' && c != '\u202f').ToArray())
                .Replace("kr", "", StringComparison.OrdinalIgnoreCase)
                .Replace(',', '.');
            return decimal.TryParse(cleaned, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount);
        }

        public static string Initials(string? givenName, string? surname)
        {
            var a = string.IsNullOrWhiteSpace(givenName) ? "" : givenName.Trim()[..1];
            var b = string.IsNullOrWhiteSpace(surname) ? "" : surname.Trim()[..1];
            var s = (a + b).ToUpperInvariant();
            return s.Length > 0 ? s : "?";
        }
    }
}
