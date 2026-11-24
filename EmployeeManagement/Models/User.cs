using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public static class WiproRoles
    {
        public const string Employee = "Employee";
        public const string Manager  = "Manager";
        public const string HR       = "HR";
        public const string CEO      = "CEO";

        public static readonly string[] All = { CEO, HR, Manager, Employee };
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>CEO | HR | Manager | Employee</summary>
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = WiproRoles.Employee;

        [StringLength(50)]
        public string? Department { get; set; }

        public int? LinkedEmployeeId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
