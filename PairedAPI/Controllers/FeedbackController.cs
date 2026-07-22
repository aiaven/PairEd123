using Microsoft.AspNetCore.Mvc;
using PairedAPI.Models;
using PairedAPI.Services;

namespace PairedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] Feedback feedback)
        {
            var success = await _feedbackService.AddFeedback(feedback);
            return success ? Ok(new { Message = "Feedback added successfully" })
                           : BadRequest(new { Message = "Failed to add feedback" });
        }

        [HttpGet("get/{sessionId}")]
        public async Task<IActionResult> Get(int sessionId)
        {
            var feedbackList = await _feedbackService.GetFeedbackBySession(sessionId);
            return Ok(feedbackList);
        }
    }
}
