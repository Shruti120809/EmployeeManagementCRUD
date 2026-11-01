using System.Data;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.API.Repositories;

public sealed class EmployeeRepository(DbConnectionFactory connectionFactory) : IEmployeeRepository
{
    private const string ProcedureName = "p_EmployeeCRUD";

    public async Task<IReadOnlyList<Employee>> GetAllEmployeesAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        var employees = new List<Employee>();
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, 4);
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 250).Value =
            string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim();

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            employees.Add(ReadEmployee(reader));
        }

        return employees;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, 5);
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = id;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadEmployee(reader) : null;
    }

    public async Task<int> AddEmployeeAsync(Employee employee, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, 1);
        AddEmployeeParameters(command, employee);
        var rowsAffected = AddRowsAffectedParameter(command);

        await connection.OpenAsync(cancellationToken);
        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is int employeeId && rowsAffected.Value is int rows && rows > 0
                ? employeeId
                : throw new InvalidOperationException("The employee could not be created.");
        }
        catch (SqlException exception) when (exception.Number is 50001 or 2601 or 2627)
        {
            throw new EmployeeEmailAlreadyExistsException();
        }
    }

    public async Task<bool> UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, 2);
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employee.EmployeeId;
        AddEmployeeParameters(command, employee);
        var rowsAffected = AddRowsAffectedParameter(command);

        await ExecuteMutationAsync(connection, command, cancellationToken);
        return rowsAffected.Value is int rows && rows > 0;
    }

    public async Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, 3);
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = id;
        var rowsAffected = AddRowsAffectedParameter(command);

        await ExecuteMutationAsync(connection, command, cancellationToken);
        return rowsAffected.Value is int rows && rows > 0;
    }

    private static SqlCommand CreateCommand(SqlConnection connection, int flag)
    {
        var command = connection.CreateCommand();
        command.CommandText = ProcedureName;
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@Flag", SqlDbType.Int).Value = flag;
        return command;
    }

    private static SqlParameter AddRowsAffectedParameter(SqlCommand command)
    {
        var parameter = command.Parameters.Add("@RowsAffected", SqlDbType.Int);
        parameter.Direction = ParameterDirection.Output;
        return parameter;
    }

    private static void AddEmployeeParameters(SqlCommand command, Employee employee)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 250).Value = employee.Name.Trim();
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 250).Value = employee.Email.Trim();
        command.Parameters.Add("@MobileNo", SqlDbType.NVarChar, 50).Value = employee.MobileNo.Trim();
        command.Parameters.Add("@Department", SqlDbType.NVarChar, 250).Value = employee.Department.Trim();
        command.Parameters.Add("@Designation", SqlDbType.NVarChar, 250).Value = employee.Designation.Trim();
    }

    private static Employee ReadEmployee(SqlDataReader reader) =>
        new()
        {
            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            Name = ReadNullableString(reader, "Name"),
            Email = ReadNullableString(reader, "Email"),
            MobileNo = ReadNullableString(reader, "MobileNo"),
            Department = ReadNullableString(reader, "Department"),
            Designation = ReadNullableString(reader, "Designation")
        };

    private static string ReadNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static async Task ExecuteMutationAsync(
        SqlConnection connection,
        SqlCommand command,
        CancellationToken cancellationToken)
    {
        await connection.OpenAsync(cancellationToken);
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException exception) when (exception.Number is 50001 or 2601 or 2627)
        {
            throw new EmployeeEmailAlreadyExistsException();
        }
    }
}
