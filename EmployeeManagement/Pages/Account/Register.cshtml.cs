using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Models;
using EmployeeManagement.Services;

namespace EmployeeManagement.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _authService.RegisterAsync(Input);

            if (result.User == null)
            {
                ErrorMessage = result.Error;
                return Page();
            }

            TempData["SuccessMessage"] = "Registration successful! Please login.";

            return RedirectToPage("/Account/Login");
        }
    }
}
