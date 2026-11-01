using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await authService.RegisterAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new
            {
                success = true,
                message = "Registration successful. You can now log in.",
                data = new { userId }
            });
        }
        catch (UserEmailAlreadyExistsException)
        {
            return Conflict(new { success = false, message = "An account with this email address already exists." });
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Unable to register user.");
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to register the account.",
                detail: "Please try again later.");
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.LoginAsync(request, cancellationToken);
            return response is null
                ? Unauthorized(new { success = false, message = "Invalid email or password." })
                : Ok(response);
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Unable to authenticate user.");
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to log in.",
                detail: "Please try again later.");
        }
    }
}
