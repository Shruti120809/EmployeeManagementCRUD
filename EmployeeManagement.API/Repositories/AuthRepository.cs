using System.Data;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.API.Repositories;

public sealed class AuthRepository(DbConnectionFactory connectionFactory) : IAuthRepository
{
    public async Task<int> RegisterAsync(User user, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.p_UserRegister";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = user.FullName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 250).Value = user.Email;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1).Value = user.PasswordHash;

        await connection.OpenAsync(cancellationToken);
        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is int userId
                ? userId
                : throw new InvalidOperationException("The user could not be registered.");
        }
        catch (SqlException exception) when (exception.Number is 50003 or 2601 or 2627)
        {
            throw new UserEmailAlreadyExistsException();
        }
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        GetUserAsync("dbo.p_UserGetByEmail", "@Email", email, cancellationToken);

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken) =>
        GetUserAsync("dbo.p_UserGetById", "@UserId", userId, cancellationToken);

    private async Task<User?> GetUserAsync(
        string procedureName,
        string parameterName,
        object parameterValue,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(parameterName, parameterValue is int ? SqlDbType.Int : SqlDbType.NVarChar,
            parameterValue is string ? 250 : 0).Value = parameterValue;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            Status = reader.GetInt32(reader.GetOrdinal("Status"))
        };
    }
}
