namespace EmployeeManagement.API.Repositories;

public sealed class EmployeeEmailAlreadyExistsException()
    : Exception("An active employee with this email address already exists.");
