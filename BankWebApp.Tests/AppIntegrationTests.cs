using System.Net;
using System.Text.RegularExpressions;
using BankWebApp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BankWebApp.Tests
{
    /// <summary>End to end through HTTP: startup seeding, logins, roles, every page, AJAX and the database.</summary>
    public class AppIntegrationTests : IClassFixture<BankAppFactory>
    {
        private readonly BankAppFactory _factory;

        public AppIntegrationTests(BankAppFactory factory) => _factory = factory;

        private HttpClient NewClient(bool followRedirects = true) =>
            _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = followRedirects, BaseAddress = new Uri("https://localhost") });

        private static string Token(string html) =>
            Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

        private static async Task<string> Text(HttpResponseMessage r) => WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());
        private static async Task<string> Text(HttpClient c, string url) => await Text(await c.GetAsync(url));

        private decimal BalanceOf(int accountId)
        {
            using var db = _factory.CreateBankContext();
            return db.Accounts.Single(a => a.AccountId == accountId).Balance;
        }

        private async Task<HttpResponseMessage> LogIn(HttpClient client, string email, string password)
        {
            var page = await client.GetStringAsync("/Identity/Account/Login");
            return await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = password,
                ["Input.RememberMe"] = "false",
                ["__RequestVerificationToken"] = Token(page)
            }));
        }

        private async Task<HttpClient> As(string email)
        {
            var client = NewClient();
            var response = await LogIn(client, email, BankAppFactory.Password);
            response.EnsureSuccessStatusCode();
            Assert.Equal("/", response.RequestMessage!.RequestUri!.AbsolutePath);
            return client;
        }

        private async Task<HttpResponseMessage> Post(HttpClient client, string url, Dictionary<string, string> fields)
        {
            fields["__RequestVerificationToken"] = Token(await client.GetStringAsync(url));
            return await client.PostAsync(url, new FormUrlEncodedContent(fields));
        }

        // ---------- Startup and database ----------

        [Fact]
        public async Task Health_check_reports_both_databases_reachable()
        {
            var response = await NewClient().GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Startup_creates_the_roles_and_the_two_readme_users()
        {
            using var scope = _factory.Services.CreateScope();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            Assert.True(await roles.RoleExistsAsync("Admin"));
            Assert.True(await roles.RoleExistsAsync("Cashier"));
            Assert.Equal(new[] { "Admin" }, await users.GetRolesAsync((await users.FindByEmailAsync(BankAppFactory.AdminEmail))!));
            Assert.Equal(new[] { "Cashier" }, await users.GetRolesAsync((await users.FindByEmailAsync(BankAppFactory.CashierEmail))!));
        }

        // ---------- Logins and roles ----------

        [Theory]
        [InlineData(BankAppFactory.AdminEmail, "Admin")]
        [InlineData(BankAppFactory.CashierEmail, "Cashier")]
        public async Task Both_readme_users_log_in_with_Hejsan123(string email, string role)
        {
            var html = await Text(await As(email), "/");

            Assert.Contains("Total balance, all accounts", html);
            Assert.Contains(role + " · " + email, html);
        }

        [Fact]
        public async Task Wrong_password_is_rejected() =>
            Assert.Contains("Invalid login attempt", await Text(await LogIn(NewClient(), BankAppFactory.CashierEmail, "hejsan123#")));

        [Theory]
        [InlineData("/Customers/Customers")]
        [InlineData("/Customers/CustomerCard/1")]
        [InlineData("/Accounts/Accounts")]
        [InlineData("/Accounts/Transactions?accountId=1001")]
        [InlineData("/Accounts/Deposit?accountId=1001")]
        [InlineData("/Accounts/Withdrawal?accountId=1001")]
        [InlineData("/Accounts/Transfer?accountId=1001")]
        [InlineData("/Countries/TopTen?countryId=SE")]
        [InlineData("/Users/Users")]
        public async Task Staff_pages_send_anonymous_visitors_to_the_login(string url)
        {
            var response = await NewClient(followRedirects: false).GetAsync(url);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("/Identity/Account/Login", response.Headers.Location!.AbsolutePath);
        }

        [Theory]
        [InlineData(BankAppFactory.CashierEmail, "/Users/Users")]
        [InlineData(BankAppFactory.AdminEmail, "/Customers/Customers")]
        [InlineData(BankAppFactory.NoRoleEmail, "/Accounts/Accounts")]
        [InlineData(BankAppFactory.NoRoleEmail, "/Countries/TopTen?countryId=SE")]
        public async Task Pages_for_another_role_are_refused(string email, string url)
        {
            var client = NewClient(followRedirects: false);
            await LogIn(client, email, BankAppFactory.Password);

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("/Identity/Account/AccessDenied", response.Headers.Location!.AbsolutePath);
        }

        // ---------- Home and statistics ----------

        [Fact]
        public async Task Public_home_page_shows_live_totals_and_countries()
        {
            var html = await Text(NewClient(), "/");

            Assert.Contains("<dt>Customers</dt><dd class=\"num\">3</dd>", html);
            Assert.Contains("<dt>Accounts</dt><dd class=\"num\">4</dd>", html);
            Assert.Contains("Sweden", html);
            Assert.DoesNotContain("anna@example.com", html); // no customer details in public
        }

        [Fact]
        public async Task Top_ten_lists_a_countrys_largest_accounts_for_staff()
        {
            var html = await Text(await As(BankAppFactory.AdminEmail), "/Countries/TopTen?countryId=SE");

            var order = new[] { "1\u00a0002", "1\u00a0001", "1\u00a0003" }.Select(n => html.IndexOf("Account " + n)).ToList();
            Assert.All(order, i => Assert.True(i > 0));
            Assert.True(order[0] < order[1] && order[1] < order[2], "Largest balance first");
            Assert.DoesNotContain("Nordmann", html);
        }

        // ---------- Customers ----------

        [Fact]
        public async Task Customer_search_finds_by_name_and_city()
        {
            var html = await Text(await As(BankAppFactory.CashierEmail), "/Customers/Customers?searchText=lindqvist+uppsala");

            Assert.Contains("Anna Lindqvist", html);
            Assert.DoesNotContain("Erik Lindqvist", html);
            Assert.Contains($"href=\"/Customers/CustomerCard/{TestDatabase.AnnaId}\"", html);
        }

        [Fact]
        public async Task Searching_a_customer_number_opens_the_profile()
        {
            var response = await (await As(BankAppFactory.CashierEmail)).GetAsync($"/Customers/Customers?searchText={TestDatabase.OlaId}");

            Assert.Equal($"/Customers/CustomerCard/{TestDatabase.OlaId}", response.RequestMessage!.RequestUri!.AbsolutePath);
        }

        [Fact]
        public async Task Customer_card_shows_details_accounts_and_total_balance()
        {
            var client = await As(BankAppFactory.CashierEmail);
            var total = BalanceOf(TestDatabase.AnnaMain) + BalanceOf(TestDatabase.AnnaSavings) + BalanceOf(TestDatabase.Shared);

            var html = await Text(client, $"/Customers/CustomerCard/{TestDatabase.AnnaId}");

            Assert.Contains("Anna Lindqvist", html);
            Assert.Contains(BankFormat.Sek(total).Replace(" kr", ""), html);
            Assert.Contains("Account 1 001", html);
            Assert.Contains("Account 1 003", html);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Customers/CustomerCard/999999")).StatusCode);
        }

        // ---------- Accounts and transactions ----------

        [Fact]
        public async Task Accounts_list_sorts_by_balance()
        {
            var html = await Text(await As(BankAppFactory.CashierEmail), "/Accounts/Accounts?sortColumn=Balance&sortOrder=desc");

            Assert.True(html.IndexOf("1 002") < html.IndexOf("2 001"));
        }

        [Fact]
        public async Task Account_shows_twenty_transactions_and_loads_twenty_more_at_a_time()
        {
            var client = await As(BankAppFactory.CashierEmail);

            var page = await Text(client, "/Accounts/Transactions?accountId=1001");
            var second = await client.GetAsync("/Accounts/Transactions?accountId=1001&handler=FetchMore&skip=20");
            var third = await client.GetAsync("/Accounts/Transactions?accountId=1001&handler=FetchMore&skip=40");

            Assert.Equal(20, Regex.Matches(page, "data-transaction=").Count);
            Assert.Contains("id=\"load-more\"", page);
            Assert.Equal(20, Regex.Matches(await Text(second), "data-transaction=").Count);
            Assert.Equal("true", second.Headers.GetValues("X-Has-More").Single());
            Assert.True(Regex.Matches(await Text(third), "data-transaction=").Count >= 5);
            Assert.DoesNotContain("<html", await Text(third)); // only rows, for appending
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Accounts/Transactions?accountId=999999")).StatusCode);
        }

        [Fact]
        public async Task Deposit_form_writes_a_credit_to_the_database()
        {
            var client = await As(BankAppFactory.CashierEmail);
            var before = BalanceOf(TestDatabase.AnnaSavings);

            var response = await Post(client, "/Accounts/Deposit?accountId=1002&customerId=1", new() { ["Amount"] = "1000.50" });

            Assert.Equal("/Accounts/Transactions", response.RequestMessage!.RequestUri!.AbsolutePath);
            Assert.Contains("Deposited 1 000,50", await Text(response));
            Assert.Equal(before + 1_000.50m, BalanceOf(TestDatabase.AnnaSavings));
        }

        [Theory]
        [InlineData("/Accounts/Withdrawal?accountId=1003", "9000", "Insufficient funds")]
        [InlineData("/Accounts/Withdrawal?accountId=1003", "50", "between 100 and 10")]
        [InlineData("/Accounts/Deposit?accountId=1003", "100.555", "at most two decimals")]
        public async Task Invalid_amounts_show_a_message_and_change_nothing(string url, string amount, string message)
        {
            var client = await As(BankAppFactory.CashierEmail);
            var before = BalanceOf(TestDatabase.Shared);

            var response = await Post(client, url, new() { ["Amount"] = amount });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(message, await Text(response));
            Assert.Equal(before, BalanceOf(TestDatabase.Shared));
        }

        [Fact]
        public async Task Transfer_form_moves_money_between_accounts()
        {
            var client = await As(BankAppFactory.CashierEmail);
            var before = (BalanceOf(TestDatabase.AnnaMain), BalanceOf(TestDatabase.OlaMain));

            var response = await Post(client, "/Accounts/Transfer?accountId=1001", new() { ["Amount"] = "125", ["AccountToId"] = "2001" });

            Assert.Equal("/Accounts/Transactions", response.RequestMessage!.RequestUri!.AbsolutePath);
            Assert.Equal(before.Item1 - 125m, BalanceOf(TestDatabase.AnnaMain));
            Assert.Equal(before.Item2 + 125m, BalanceOf(TestDatabase.OlaMain));
        }

        [Theory]
        [InlineData("1001", "Choose a different account")]
        [InlineData("99999", "There is no account with that number")]
        public async Task Transfer_to_the_same_or_a_missing_account_is_explained(string to, string message)
        {
            var client = await As(BankAppFactory.CashierEmail);

            var html = await Text(await Post(client, "/Accounts/Transfer?accountId=1001", new() { ["Amount"] = "100", ["AccountToId"] = to }));

            Assert.Contains(message, html);
        }

        // ---------- Users ----------

        [Fact]
        public async Task Admin_sees_every_login_and_its_role()
        {
            var html = await Text(await As(BankAppFactory.AdminEmail), "/Users/Users");

            Assert.Contains(BankAppFactory.CashierEmail, html);
            Assert.Contains("No role: can't open customers or accounts", html);
        }

        [Fact]
        public async Task Logout_ends_the_session()
        {
            var client = await As(BankAppFactory.CashierEmail);
            var home = await client.GetStringAsync("/");

            await client.PostAsync("/Identity/Account/Logout?returnUrl=%2F", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = Token(home) }));
            var after = await client.GetAsync("/Customers/Customers");

            Assert.StartsWith("/Identity/Account/Login", after.RequestMessage!.RequestUri!.AbsolutePath);
        }
    }
}
