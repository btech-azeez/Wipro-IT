using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;

namespace EmployeeManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<User>     Users     { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.WiproEmployeeId).IsUnique();
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // ── Seed Employees ───────────────────────────────────────────
            modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                EmployeeId       = 1,
                WiproEmployeeId  = "WIP-CEO-001",
                FirstName        = "Casey",
                LastName         = "Morgan",
                Email            = "casey.morgan@example.test",
                Department       = "Executive",
                Designation      = "Executive",
                Role             = WiproRoles.CEO,
                Salary           = 5000000,
                DateOfJoining    = new DateTime(2023, 4, 6, 0, 0, 0, DateTimeKind.Utc),
                WorkLocation     = "Remote",
                IsActive         = true,
                Gender           = "Prefer not to say",
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Employee
            {
                EmployeeId       = 2,
                WiproEmployeeId  = "WIP-HR-001",
                FirstName        = "Riley",
                LastName         = "Quinn",
                Email            = "riley.quinn@example.test",
                Department       = "Human Resources",
                Designation      = "People Operations Lead",
                Role             = WiproRoles.HR,
                Salary           = 1500000,
                DateOfJoining    = new DateTime(2018, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                WorkLocation     = "Remote",
                IsActive         = true,
                Gender           = "Prefer not to say",
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Employee
            {
                EmployeeId       = 3,
                WiproEmployeeId  = "WIP-ENG-001",
                FirstName        = "Jordan",
                LastName         = "Lee",
                Email            = "jordan.lee@example.test",
                Department       = "Engineering",
                Designation      = "Engineering Manager",
                Role             = WiproRoles.Manager,
                Salary           = 1800000,
                DateOfJoining    = new DateTime(2016, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                WorkLocation     = "Pune",
                IsActive         = true,
                Gender           = "Prefer not to say",
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Employee
            {
                EmployeeId       = 4,
                WiproEmployeeId  = "WIP-ENG-002",
                FirstName        = "Alex",
                LastName         = "Reed",
                Email            = "alex.reed@example.test",
                PhoneNumber      = null,
                Department       = "Engineering",
                Designation      = "Senior Software Engineer",
                Role             = WiproRoles.Employee,
                Salary           = 1200000,
                DateOfJoining    = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                WorkLocation     = "Remote",
                ReportingManagerId = "WIP-ENG-001",
                IsActive         = true,
                Gender           = "Prefer not to say",
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Employee
            {
                EmployeeId       = 5,
                WiproEmployeeId  = "WIP-ENG-003",
                FirstName        = "Taylor",
                LastName         = "Park",
                Email            = "taylor.park@example.test",
                PhoneNumber      = null,
                Department       = "Engineering",
                Designation      = "Software Engineer",
                Role             = WiproRoles.Employee,
                Salary           = 800000,
                DateOfJoining    = new DateTime(2022, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                WorkLocation     = "Remote",
                ReportingManagerId = "WIP-ENG-001",
                IsActive         = true,
                Gender           = "Prefer not to say",
                CreatedAt        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
            );
        }
    }
}
