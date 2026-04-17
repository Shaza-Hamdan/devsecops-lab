using GitFile.DTO;
using Registration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InformationSecurity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService registrationservice;

        public RegistrationController(IRegistrationService registrationService)
        {
            registrationservice = registrationService;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateNewAccountRequest account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await registrationservice.Register(account);
                return Ok("Registration successful");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest account)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(account.Email) || string.IsNullOrEmpty(account.Password))
            {
                throw new ArgumentException("Email and Password must not be empty.");
            }

            try
            {
                var token = registrationservice.Login(account);
                if (token == "NotFound")
                {
                    return NotFound("User not found.");
                }
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Login failed. Invalid token.");
                }

                return Ok(token);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid email or password.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}