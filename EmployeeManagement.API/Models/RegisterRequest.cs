using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(250)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,128}$",
        ErrorMessage = "Password must contain at least 8 characters, including uppercase, lowercase, a number, and a special character.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation must match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
