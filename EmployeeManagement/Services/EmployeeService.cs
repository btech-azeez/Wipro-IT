using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace EmployeeManagement.Services
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByWiproIdAsync(string wiproId);
        Task<Employee> CreateAsync(Employee employee);
        Task<Employee?> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(int id);
        Task<(List<Employee> Employees, int TotalCount)> GetPagedAsync(
            int page, int pageSize,
            string? search = null, string? department = null, string? role = null);
        Task<List<string>> GetDepartmentsAsync();
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> WiproIdExistsAsync(string wiproId, int? excludeId = null);
        Task<DashboardStats> GetDashboardStatsAsync();

        /// <summary>Returns employees visible to the given user based on RBAC.</summary>
        Task<List<Employee>> GetVisibleEmployeesAsync(string role, string? department);
    }

    public class DashboardStats
    {
        public int Total              { get; set; }
        public int Active             { get; set; }
        public int DepartmentCount    { get; set; }
        public int RecentHires        { get; set; }
        public int CEOCount           { get; set; }
        public int HRCount            { get; set; }
        public int ManagerCount       { get; set; }
        public int EmployeeCount      { get; set; }
        public decimal TotalPayroll   { get; set; }
        public List<(string Dept, int Count)> DeptBreakdown { get; set; } = new();
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<EmployeeNotificationHub> _notifications;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            AppDbContext context,
            IHubContext<EmployeeNotificationHub> notifications,
            ILogger<EmployeeService> logger)
        {
            _context = context;
            _notifications = notifications;
            _logger = logger;
        }

        public Task<List<Employee>> GetAllAsync()
            => _context.Employees.OrderByDescending(e => e.CreatedAt).ToListAsync();

        public Task<Employee?> GetByIdAsync(int id)
            => _context.Employees.FindAsync(id).AsTask()!;

        public Task<Employee?> GetByWiproIdAsync(string wiproId)
            => _context.Employees.FirstOrDefaultAsync(e => e.WiproEmployeeId == wiproId);

        public async Task<Employee> CreateAsync(Employee employee)
        {
            employee.CreatedAt = DateTime.UtcNow;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            await NotifyEmployeeChangeAsync("Employee added", $"{employee.FirstName} {employee.LastName} joined the directory.");
            return employee;
        }

        public async Task<Employee?> UpdateAsync(Employee employee)
        {
            var ex = await _context.Employees.FindAsync(employee.EmployeeId);
            if (ex == null) return null;

            ex.WiproEmployeeId   = employee.WiproEmployeeId;
            ex.FirstName         = employee.FirstName;
            ex.LastName          = employee.LastName;
            ex.Email             = employee.Email;
            ex.PhoneNumber       = employee.PhoneNumber;
            ex.Department        = employee.Department;
            ex.Designation       = employee.Designation;
            ex.Role              = employee.Role;
            ex.Salary            = employee.Salary;
            ex.DateOfJoining     = employee.DateOfJoining;
            ex.WorkLocation      = employee.WorkLocation;
            ex.ReportingManagerId = employee.ReportingManagerId;
            ex.BloodGroup        = employee.BloodGroup;
            ex.DateOfBirth       = employee.DateOfBirth;
            ex.Gender            = employee.Gender;
            ex.IsActive          = employee.IsActive;
            ex.Address           = employee.Address;
            ex.UpdatedAt         = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await NotifyEmployeeChangeAsync("Employee updated", $"{ex.FirstName} {ex.LastName}'s record was updated.");
            return ex;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return false;
            var name = $"{emp.FirstName} {emp.LastName}";
            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            await NotifyEmployeeChangeAsync("Employee removed", $"{name} was removed from the directory.");
            return true;
        }

        private async Task NotifyEmployeeChangeAsync(string title, string message)
        {
            try
            {
                await _notifications.Clients.All.SendAsync("EmployeeChanged", new
                {
                    title,
                    message,
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to deliver employee change notification.");
            }
        }

        public async Task<(List<Employee> Employees, int TotalCount)> GetPagedAsync(
            int page, int pageSize,
            string? search = null, string? department = null, string? role = null)
        {
            var q = _context.Employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search)  ||
                    e.Email.Contains(search)     ||
                    e.WiproEmployeeId.Contains(search));

            if (!string.IsNullOrWhiteSpace(department))
                q = q.Where(e => e.Department == department);

            if (!string.IsNullOrWhiteSpace(role))
                q = q.Where(e => e.Role == role);

            var total = await q.CountAsync();
            var list  = await q.OrderByDescending(e => e.CreatedAt)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
            return (list, total);
        }

        public Task<List<string>> GetDepartmentsAsync()
            => _context.Employees.Select(e => e.Department).Distinct().OrderBy(d => d).ToListAsync();

        public Task<bool> EmailExistsAsync(string email, int? excludeId = null)
            => _context.Employees.AnyAsync(e => e.Email == email && (excludeId == null || e.EmployeeId != excludeId));

        public Task<bool> WiproIdExistsAsync(string wiproId, int? excludeId = null)
            => _context.Employees.AnyAsync(e => e.WiproEmployeeId == wiproId && (excludeId == null || e.EmployeeId != excludeId));

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var all = await _context.Employees.ToListAsync();
            var deptBreakdown = all.GroupBy(e => e.Department)
                                    .Select(g => (g.Key, g.Count()))
                                    .OrderByDescending(x => x.Item2)
                                    .ToList();
            return new DashboardStats
            {
                Total           = all.Count,
                Active          = all.Count(e => e.IsActive),
                DepartmentCount = all.Select(e => e.Department).Distinct().Count(),
                RecentHires     = all.Count(e => e.DateOfJoining >= DateTime.UtcNow.AddDays(-30)),
                CEOCount        = all.Count(e => e.Role == WiproRoles.CEO),
                HRCount         = all.Count(e => e.Role == WiproRoles.HR),
                ManagerCount    = all.Count(e => e.Role == WiproRoles.Manager),
                EmployeeCount   = all.Count(e => e.Role == WiproRoles.Employee),
                TotalPayroll    = all.Where(e => e.IsActive).Sum(e => e.Salary),
                DeptBreakdown   = deptBreakdown
            };
        }

        public Task<List<Employee>> GetVisibleEmployeesAsync(string role, string? department)
        {
            // CEO & HR can see everyone
            // Manager sees only their department
            // Employee sees only themselves (handled via page-level logic)
            var q = _context.Employees.AsQueryable();
            if (role == WiproRoles.Manager && !string.IsNullOrEmpty(department))
                q = q.Where(e => e.Department == department);
            return q.OrderByDescending(e => e.CreatedAt).ToListAsync();
        }
    }
}
