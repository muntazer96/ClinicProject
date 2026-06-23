using Clinic_Booking.DTOs.MessageDTO;
using Clinic_Booking.IServices.IMessageServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    [Authorize]
    [EnableRateLimiting("Messaging")]
    public class MessageController : BaseApiController
    {
        private readonly IMessageServices _services;

        public MessageController(IMessageServices services)
        {
            _services = services;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendAsync([FromBody] SendMessageDto form)
        {
            return await _services.SendAsync(form);
        }

        [HttpPost("send-image")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> SendImageAsync([FromForm] SendImageMessageDto form)
        {
            return await _services.SendImageAsync(form.ReceiverId, form.File, form.Content);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversationsAsync()
        {
            return await _services.GetConversationsAsync();
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversationAsync(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            return await _services.GetConversationAsync(otherUserId, page, pageSize);
        }

        [HttpPut("read/{otherUserId}")]
        public async Task<IActionResult> MarkAsReadAsync(Guid otherUserId)
        {
            return await _services.MarkAsReadAsync(otherUserId);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCountAsync()
        {
            return await _services.GetUnreadCountAsync();
        }

        [HttpGet("can-send/{userId}")]
        public async Task<IActionResult> CanSendAsync(Guid userId)
        {
            var canSend = await _services.CanCurrentUserSendMessageAsync(userId);
            return Ok(new { canSend });
        }
    }
}
