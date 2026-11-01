using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Interfaces;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllEmployeesAsync(string? search, CancellationToken cancellationToken);
    Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken);
    Task<int> AddEmployeeAsync(Employee employee, CancellationToken cancellationToken);
    Task<bool> UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken);
    Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken);
}
