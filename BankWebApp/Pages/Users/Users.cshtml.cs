using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankWebApp.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersModel(UserManager<IdentityUser> userManager) => _userManager = userManager;

        public List<UserRow> Users { get; set; } = new();

        public record UserRow(string Email, IList<string> Roles, bool EmailConfirmed, bool LockedOut);

        public async Task OnGetAsync()
        {
            foreach (var user in _userManager.Users.OrderBy(u => u.Email).ToList())
            {
                Users.Add(new UserRow(
                    user.Email ?? user.UserName ?? "",
                    await _userManager.GetRolesAsync(user),
                    user.EmailConfirmed,
                    await _userManager.IsLockedOutAsync(user)));
            }
        }
    }
}
