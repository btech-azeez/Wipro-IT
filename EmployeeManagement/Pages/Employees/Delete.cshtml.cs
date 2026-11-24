using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Models;
using EmployeeManagement.Services;

namespace EmployeeManagement.Pages.Employees
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class DeleteModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        [BindProperty]
        public Employee Employee { get; set; } = new();

        public DeleteModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
                return NotFound();

            Employee = employee;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _employeeService.DeleteAsync(Employee.EmployeeId);
            if (result)
                TempData["SuccessMessage"] = "Employee deleted successfully.";
            else
                TempData["ErrorMessage"] = "Failed to delete employee.";

            return RedirectToPage("Index");
        }
    }
}
