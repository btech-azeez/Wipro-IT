using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Data
{
    public static class EmployeeDataSeeder
    {
        private const int FirstSequence = 6;
        private const int EmployeeCount = 100;

        public static async Task SeedAsync(AppDbContext db)
        {
            var existingIds = await db.Employees
                .Where(employee => employee.WiproEmployeeId.StartsWith("WIP-BATCH-"))
                .Select(employee => employee.WiproEmployeeId)
                .ToHashSetAsync();

            var currentMaxId = await db.Employees.MaxAsync(employee => (int?)employee.EmployeeId) ?? 0;
            var firstNameParts = new[] { "Aanya", "Arjun", "Dev", "Isha", "Kabir", "Mira", "Neel", "Riya", "Vihaan", "Zoya" };
            var lastNameParts = new[] { "Adiga", "Bansal", "Chawla", "Das", "Iyer", "Joshi", "Kapoor", "Nair", "Rao", "Singh" };
            var departments = new[] { "Engineering", "Human Resources", "Sales", "Finance", "Marketing", "Operations", "Legal", "IT Infrastructure", "Quality Assurance", "Product" };
            var locations = new[] { "Bangalore", "Hyderabad", "Pune", "Chennai", "Mumbai" };
            var newEmployees = new List<Employee>();

            for (var sequence = FirstSequence; sequence < FirstSequence + EmployeeCount; sequence++)
            {
                var wiproId = $"WIP-BATCH-{sequence:D3}";
                if (existingIds.Contains(wiproId))
                    continue;

                var nameIndex = sequence - FirstSequence;
                newEmployees.Add(new Employee
                {
                    WiproEmployeeId = wiproId,
                    FirstName = firstNameParts[nameIndex / lastNameParts.Length],
                    LastName = lastNameParts[nameIndex % lastNameParts.Length],
                    Email = $"employee{sequence:D3}@example.test",
                    PhoneNumber = null,
                    Department = departments[nameIndex % departments.Length],
                    Designation = "Associate",
                    Role = WiproRoles.Employee,
                    Salary = 600000 + (sequence % 12 * 100000),
                    DateOfJoining = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(sequence),
                    WorkLocation = locations[nameIndex % locations.Length],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (newEmployees.Count == 0)
                return;

            db.Employees.AddRange(newEmployees);
            await db.SaveChangesAsync();
        }
    }
}