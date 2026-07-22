using Microsoft.AspNetCore.Mvc;
using PairedAPI.Models;
using PairedAPI.Services;

namespace PairedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] Message message)
        {
            var success = await _messageService.SendMessage(message);
            return success ? Ok(new { Message = "Message sent successfully" })
                           : BadRequest(new { Message = "Failed to send message" });
        }

        [HttpGet("conversation/{user1Id}/{user2Id}")]
        public async Task<IActionResult> GetConversation(int user1Id, int user2Id)
        {
            var messages = await _messageService.GetConversation(user1Id, user2Id);
            return Ok(messages);
        }
    }
}
