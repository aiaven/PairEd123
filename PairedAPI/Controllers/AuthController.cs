using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using PairedAPI.Models;
using PairedAPI.Services;

namespace PairedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var success = await _authService.RegisterUser(user);
            if (success)
                return Ok(new { Message = "User registered successfully" });
            else
                return BadRequest(new { Message = "Registration failed" });
        }

        [HttpPost("add-skill")]
        public async Task<IActionResult> AddSkill([FromBody] SkillRequest request)
        {
            var success = await _authService.AddSkill(request.UserId, request.SkillName);
            return success ? Ok(new { Message = "Skill added successfully" })
                           : BadRequest(new { Message = "Failed to add skill" });
        }

        [HttpDelete("remove-skill")]
        public async Task<IActionResult> RemoveSkill([FromBody] SkillRequest request)
        {
            var success = await _authService.RemoveSkill(request.UserId, request.SkillName);
            return success ? Ok(new { Message = "Skill removed successfully" })
                           : BadRequest(new { Message = "Failed to remove skill" });
        }

        [HttpGet("get-skills/{userId}")]
        public async Task<IActionResult> GetSkills(int userId)
        {
            var skills = await _authService.GetSkills(userId);
            return Ok(skills);
        }


        [HttpPut("opt-in-tutor")]
        public async Task<IActionResult> OptInTutor([FromBody] User user)
        {
            var success = await _authService.OptInAsTutor(user.UserId, user.IsTutor);
            if (success)
                return Ok(new { Message = "Tutor status updated successfully" });
            else
                return BadRequest(new { Message = "Tutor opt-in failed" });
        }


        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User user)
        {
            var success = await _authService.UpdateProfile(user.UserId, user.DisplayName, user.Availability);
            if (success)
                return Ok(new { Message = "Profile updated successfully" });
            else
                return BadRequest(new { Message = "Update failed" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PairedAPI.Models.LoginRequest login)
        {
            var loggedInUser = await _authService.LoginUser(login.Email, login.Password);
            if (loggedInUser != null)
                return Ok(new { Message = "Login successful", User = loggedInUser });
            else
                return Unauthorized(new { Message = "Invalid credentials" });
        }


    }
}
