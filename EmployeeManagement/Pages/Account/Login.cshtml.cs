using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAuthService _auth;
        private readonly IJwtService  _jwt;

        [BindProperty] public LoginViewModel Input { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? ReturnUrl    { get; set; }

        public LoginModel(IAuthService auth, IJwtService jwt)
        {
            _auth = auth;
            _jwt  = jwt;
        }

        public void OnGet(string? returnUrl = null)
            => ReturnUrl = returnUrl ?? Url.Content("~/");

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid) return Page();

            var user = await _auth.AuthenticateAsync(Input.Username, Input.Password);
            if (user == null)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            // ── Cookie auth claims ────────────────────────────────────────
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name,            user.Username),
                new(ClaimTypes.Email,           user.Email),
                new(ClaimTypes.Role,            user.Role),
                new("FullName",                 user.FullName),
                new("Department",               user.Department ?? "")
            };

            var identity   = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProps  = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                ExpiresUtc   = Input.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProps);

            // ── JWT stored in session for API calls ───────────────────────
            var jwtToken     = _jwt.GenerateToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();
            await _auth.UpdateRefreshTokenAsync(user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

            HttpContext.Session.SetString("JwtToken",     jwtToken);
            HttpContext.Session.SetString("RefreshToken", refreshToken);
            HttpContext.Session.SetString("Username",     user.Username);
            HttpContext.Session.SetString("UserRole",     user.Role);
            HttpContext.Session.SetString("FullName",     user.FullName);
            HttpContext.Session.SetInt32 ("UserId",       user.UserId);

            TempData["SuccessMessage"] = $"Welcome back, {user.FullName}! 👋";
            return LocalRedirect(returnUrl);
        }
    }
}
