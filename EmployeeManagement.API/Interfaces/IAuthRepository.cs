using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Interfaces;

public interface IAuthRepository
{
    Task<int> RegisterAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken);
}
