using System.Security.Claims;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeManagement.Pages.Admin
{
    [Authorize(Roles = "CEO,HR")]
    public class UsersModel : PageModel
    {
        private readonly IAuthService _authService;

        public List<User> Users { get; private set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public UsersModel(IAuthService authService) => _authService = authService;

        public async Task OnGetAsync()
        {
            var users = await _authService.GetAllUsersAsync();
            if (!string.IsNullOrWhiteSpace(Search))
            {
                users = users.Where(user =>
                    user.FullName.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                    user.Email.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                    user.Role.Contains(Search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            Users = users;
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            if (IsCurrentUser(id))
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
            }
            else
            {
                TempData["SuccessMessage"] = await _authService.ToggleUserStatusAsync(id)
                    ? "Account status updated."
                    : "Account was not found.";
            }

            return RedirectToPage(new { Search });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (IsCurrentUser(id))
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
            }
            else
            {
                TempData["SuccessMessage"] = await _authService.DeleteUserAsync(id)
                    ? "Account deleted."
                    : "Account was not found.";
            }

            return RedirectToPage(new { Search });
        }

        private bool IsCurrentUser(int id)
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId)
                && currentUserId == id;
    }
}