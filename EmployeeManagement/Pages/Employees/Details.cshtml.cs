using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Employees
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IEmployeeService _svc;
        public Employee Employee { get; set; } = new();

        public DetailsModel(IEmployeeService svc) => _svc = svc;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var emp = await _svc.GetByIdAsync(id);
            if (emp == null) return NotFound();

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Employees can only view their own profile (linked by email)
            if (role == WiproRoles.Employee)
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                if (!emp.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase))
                    return RedirectToPage("/Account/AccessDenied");
            }

            Employee = emp;
            return Page();
        }
    }
}
