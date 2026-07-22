using Microsoft.AspNetCore.Mvc;
using PairedAPI.Models;
using PairedAPI.Services;

namespace PairedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly RequestService _requestService;

        public RequestsController(RequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet("get-for-tutor/{tutorId}")]
        public async Task<IActionResult> GetForTutor(int tutorId)
        {
            var requests = await _requestService.GetRequestsForTutor(tutorId);
            return Ok(requests);
        }

        [HttpGet("get-by-status/{userId}/{role}/{status}")]
        public async Task<IActionResult> GetByStatus(int userId, string role, string status)
        {
            var requests = await _requestService.GetRequestsByStatus(userId, role, status);
            return Ok(requests);
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Request request)
        {
            if (request.TutorId <= 0 || request.TuteeId <= 0)
                return BadRequest("TutorId and TuteeId are required.");

            if (request.PreferredDate <= DateTime.UtcNow)
                return BadRequest("Preferred date must be in the future.");

            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest("Subject is required.");

            var result = await _requestService.CreateRequest(request);
            return Ok(result);
        }


        [HttpGet("get/{tuteeId}")]
        public async Task<IActionResult> Get(int tuteeId)
        {
            var requests = await _requestService.GetRequests(tuteeId);
            return Ok(requests);
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] Request request)
        {
            var success = await _requestService.UpdateRequestStatus(request.RequestId, request.Status, request.RejectionReason);
            return success ? Ok(new { Message = "Request status updated successfully" })
                           : BadRequest(new { Message = "Failed to update request status" });
        }
    }
}
