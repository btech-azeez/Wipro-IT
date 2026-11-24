using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Employees
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _svc;

        public List<Employee> Employees { get; set; } = new();
        public int TotalCount  { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize    { get; set; } = 15;
        public int TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);

        [BindProperty(SupportsGet = true)] public string? SearchTerm  { get; set; }
        [BindProperty(SupportsGet = true)] public string? Department   { get; set; }
        [BindProperty(SupportsGet = true)] public string? RoleFilter   { get; set; }

        public List<string> Departments { get; set; } = new();

        public IndexModel(IEmployeeService svc) => _svc = svc;

        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = page;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var dept = User.FindFirst("Department")?.Value;

            // Managers only see their own department
            if (role == WiproRoles.Manager && string.IsNullOrEmpty(Department))
                Department = dept;

            var result = await _svc.GetPagedAsync(CurrentPage, PageSize, SearchTerm, Department, RoleFilter);
            Employees   = result.Employees;
            TotalCount  = result.TotalCount;
            Departments = await _svc.GetDepartmentsAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (role != WiproRoles.CEO && role != WiproRoles.HR)
            {
                TempData["ErrorMessage"] = "Access denied. Only CEO and HR can delete employees.";
                return RedirectToPage();
            }

            var ok = await _svc.DeleteAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
                ok ? "Employee removed successfully." : "Failed to remove employee.";
            return RedirectToPage();
        }
    }
}
