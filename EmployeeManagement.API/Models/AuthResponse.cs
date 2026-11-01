namespace EmployeeManagement.API.Models;

public sealed class AuthResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public AuthenticatedUser? Data { get; init; }
}

public sealed class AuthenticatedUser
{
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}
