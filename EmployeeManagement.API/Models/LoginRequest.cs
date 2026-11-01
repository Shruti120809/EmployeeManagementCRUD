using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(250)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
