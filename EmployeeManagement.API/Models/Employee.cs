using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public sealed class Employee
{
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(250, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(250)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must contain exactly 10 digits.")]
    [StringLength(50)]
    public string MobileNo { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Designation { get; set; } = string.Empty;
}
