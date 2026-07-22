using Microsoft.AspNetCore.Mvc;
using PairedAPI.Models;
using PairedAPI.Services;

namespace PairedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly SessionService _sessionService;

        public SessionController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Session session)
        {
            var success = await _sessionService.CreateSession(session);
            return success ? Ok(new { Message = "Session created successfully" })
                           : BadRequest(new { Message = "Failed to create session" });
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] Session session)
        {
            var success = await _sessionService.UpdateSessionStatus(session.SessionId, session.Status);
            return success ? Ok(new { Message = "Session status updated successfully" })
                           : BadRequest(new { Message = "Failed to update session status" });
        }

        [HttpGet("get/{userId}")]
        public async Task<IActionResult> Get(int userId)
        {
            var sessions = await _sessionService.GetSessions(userId);
            return Ok(sessions);
        }
    }
}
