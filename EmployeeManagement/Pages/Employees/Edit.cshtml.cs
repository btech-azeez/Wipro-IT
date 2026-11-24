using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Models;
using EmployeeManagement.Services;

namespace EmployeeManagement.Pages.Employees
{
    [Authorize(Policy = "ManagerOrAbove")]
    public class EditModel : PageModel
    {
        private readonly IEmployeeService _svc;

        [BindProperty] public Employee Employee { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public EditModel(IEmployeeService svc) => _svc = svc;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var emp = await _svc.GetByIdAsync(id);
            if (emp == null) return NotFound();
            Employee = emp;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (await _svc.EmailExistsAsync(Employee.Email, Employee.EmployeeId))
            { ErrorMessage = "Another employee with this email already exists."; return Page(); }

            if (await _svc.WiproIdExistsAsync(Employee.WiproEmployeeId, Employee.EmployeeId))
            { ErrorMessage = "Another employee with this Wipro ID already exists."; return Page(); }

            var result = await _svc.UpdateAsync(Employee);
            if (result == null) return NotFound();

            TempData["SuccessMessage"] = "Employee record updated successfully.";
            return RedirectToPage("Index");
        }
    }
}
