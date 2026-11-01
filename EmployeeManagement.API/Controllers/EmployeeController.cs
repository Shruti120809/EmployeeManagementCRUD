using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class EmployeeController(
    IEmployeeRepository employeeRepository,
    ILogger<EmployeeController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Employee>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Employee>>> GetAll([FromQuery] string? search,CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await employeeRepository.GetAllEmployeesAsync(search, cancellationToken));
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Unable to retrieve employees.");
            return Problem(statusCode: StatusCodes.Status500InternalServerError,title: "Unable to retrieve employees.", detail: "Please try again later.");
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Employee>> GetById(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Employee ID must be greater than zero." });
        }

        try
        {
            var employee = await employeeRepository.GetEmployeeByIdAsync(id, cancellationToken);
            return employee is null ? NotFound(new { message = "Employee was not found." }) : Ok(employee);
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Unable to retrieve employee {EmployeeId}.", id);
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Unable to retrieve the employee.", detail: "Please try again later.");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(Employee employee, CancellationToken cancellationToken)
    {
        try
        {
            var id = await employeeRepository.AddEmployeeAsync(employee, cancellationToken);
            employee.EmployeeId = id;
            return CreatedAtAction(nameof(GetById), new { id }, employee);
        }
        catch (EmployeeEmailAlreadyExistsException)
        {
            return Conflict(new { message = "An active employee with this email address already exists." });
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Unable to create employee.");
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to create the employee.",
                detail: "Please try again later.");
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, Employee employee, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Employee ID must be greater than zero." });
        }

        employee.EmployeeId = id;
        try
        {
            var updated = await employeeRepository.UpdateEmployeeAsync(employee, cancellationToken);
            return updated
                ? Ok(new { message = "Employee updated successfully." })
                : NotFound(new { message = "Employee was not found." });
        }
        catch (EmployeeEmailAlreadyExistsException)
        {
            return Conflict(new { message = "An active employee with this email address already exists." });
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Unable to update employee {EmployeeId}.", id);
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to update the employee.",
                detail: "Please try again later.");
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Employee ID must be greater than zero." });
        }

        try
        {
            var deleted = await employeeRepository.DeleteEmployeeAsync(id, cancellationToken);
            return deleted
                ? Ok(new { message = "Employee deleted successfully." })
                : NotFound(new { message = "Employee was not found." });
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Unable to delete employee {EmployeeId}.", id);
            return Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Unable to delete the employee.",
                detail: "Please try again later.");
        }
    }
}
