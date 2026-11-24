using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        [StringLength(20)]
        [Display(Name = "Wipro Employee ID")]
        public string WiproEmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Work Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(15)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation is required")]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        /// <summary>Employee | Manager | HR | CEO</summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Role")]
        public string Role { get; set; } = WiproRoles.Employee;

        [Required]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be positive")]
        [Display(Name = "Annual CTC (₹)")]
        public decimal Salary { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Joining")]
        public DateTime DateOfJoining { get; set; }

        [StringLength(50)]
        [Display(Name = "Work Location")]
        public string? WorkLocation { get; set; }

        [StringLength(50)]
        [Display(Name = "Reporting Manager ID")]
        public string? ReportingManagerId { get; set; }

        [StringLength(10)]
        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        public bool IsActive { get; set; } = true;

        [DataType(DataType.MultilineText)]
        public string? Address { get; set; }

        [StringLength(200)]
        [Display(Name = "Profile Photo URL")]
        public string? ProfilePhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
