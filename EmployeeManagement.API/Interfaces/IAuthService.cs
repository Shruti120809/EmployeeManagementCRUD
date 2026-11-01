using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Interfaces;

public interface IAuthService
{
    Task<int> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
