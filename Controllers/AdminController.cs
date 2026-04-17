using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _AdminService;
    private readonly IRoleService _roleService;

    public AdminController(IAdminService adminService, IRoleService roleService)
    {
        _AdminService = adminService;
        _roleService = roleService;
    }


    [HttpGet("getUsers")]
    public async Task<IActionResult> GetAllRegistrations()
    {
        try
        {
            var registrations = await _AdminService.GetAllRegistrationsAsync();
            if (registrations == null || !registrations.Any())
            {
                return NotFound("No registrations found.");
            }

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

            return Ok(registrations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpPut("defineUserRole (Admin, User, Guest)")]
    public async Task<IActionResult> UpdateRegistrationRole(Guid id, [FromBody] UpdateRoleRequest roleRequest)
    {
        if (roleRequest == null || !ModelState.IsValid)
        {
            return BadRequest("Invalid role information.");
        }

        try
        {
            var registration = await _AdminService.GetRegistrationByIdAsync(id);
            if (registration == null)
            {
                return NotFound($"Registration with ID {id} not found.");
            }

            var role = await _roleService.GetRoleByNameAsync(roleRequest.NewRole);
            if (role == null)
            {
                return BadRequest("Invalid role.");
            }

            registration.roleId = role.Id;
            await _AdminService.UpdateRegistrationAsync(registration);

            return Ok("Registration role updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpDelete("deleteUser")]
    public async Task<IActionResult> DeleteRegistration(Guid id)
    {
        try
        {
            bool result = await _AdminService.DeleteRegistrationAsync(id);
            if (!result)
            {
                return NotFound($"Registration with ID {id} not found.");
            }
            return Ok("Registration deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}
