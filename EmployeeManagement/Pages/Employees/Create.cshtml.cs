using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Models;
using EmployeeManagement.Services;

namespace EmployeeManagement.Pages.Employees
{
    [Authorize(Policy = "ManagerOrAbove")]
    public class CreateModel : PageModel
    {
        private readonly IEmployeeService _svc;

        [BindProperty]
        public Employee Employee { get; set; } = new() { DateOfJoining = DateTime.Today, IsActive = true };

        public string? ErrorMessage { get; set; }

        public CreateModel(IEmployeeService svc) => _svc = svc;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (await _svc.EmailExistsAsync(Employee.Email))
            { ErrorMessage = "An employee with this email already exists."; return Page(); }

            if (await _svc.WiproIdExistsAsync(Employee.WiproEmployeeId))
            { ErrorMessage = "An employee with this Wipro ID already exists."; return Page(); }

            await _svc.CreateAsync(Employee);
            TempData["SuccessMessage"] = $"Employee {Employee.FirstName} {Employee.LastName} added successfully!";
            return RedirectToPage("Index");
        }
    }
}
