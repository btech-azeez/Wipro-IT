using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeManagement.Services;

namespace EmployeeManagement.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _svc;
        public DashboardStats Stats { get; set; } = new();

        public IndexModel(IEmployeeService svc) => _svc = svc;

        public async Task OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
                Stats = await _svc.GetDashboardStatsAsync();
        }
    }
}
