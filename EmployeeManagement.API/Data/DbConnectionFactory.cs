using Microsoft.Data.SqlClient;

namespace EmployeeManagement.API.Data;

public sealed class DbConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("EmployeeManagement")
        ?? throw new InvalidOperationException("The connection string is not configured.");

    public SqlConnection CreateConnection() => new(_connectionString);
}
