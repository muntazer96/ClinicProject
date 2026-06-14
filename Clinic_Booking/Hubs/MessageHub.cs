using System.Security.Claims;
using Clinic_Booking.Data;
using Clinic_Booking.DTOs.MessageDTO;
using Clinic_Booking.Entities.Message;
using Clinic_Booking.IServices.IMessageServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMessageServices _messageServices;
        private readonly ApplicationDbContext _context;

        public MessageHub(
            IMessageServices messageServices,
            ApplicationDbContext context)
        {
            _messageServices = messageServices;
            _context = context;
        }

        private Guid? GetUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var userId))
                return userId;
            return null;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");

                var unreadCount = await _messageServices.GetUnreadCountForUserAsync(userId.Value);
                await Clients.Caller.SendAsync("UnreadCount", unreadCount);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue && userId.Value != Guid.Empty)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDto form)
        {
            var userId = GetUserId();
            if (userId == null || userId == Guid.Empty)
            {
                await Clients.Caller.SendAsync("Error", "You must be authenticated to send messages.");
                return;
            }

            if (form.ReceiverId == userId.Value)
            {
                await Clients.Caller.SendAsync("Error", "Cannot send a message to yourself.");
                return;
            }

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == form.ReceiverId);
            if (!receiverExists)
            {
                await Clients.Caller.SendAsync("Error", "Receiver not found.");
                return;
            }

            var message = new Message
            {
                SenderId = userId.Value,
                ReceiverId = form.ReceiverId,
                Content = form.Content.Trim(),
                SentAt = DateTime.UtcNow,
                Type = form.Type,
                IsRead = false
            };

            var saved = await _messageServices.SaveAndReturnAsync(message);
            var dto = await _messageServices.GetMessageDtoAsync(saved.Id);

            await Clients.Caller.SendAsync("MessageSent", dto);
            await Clients.User(form.ReceiverId.ToString()).SendAsync("ReceiveMessage", dto);

            var unreadCount = await _messageServices.GetUnreadCountForUserAsync(form.ReceiverId);
            await Clients.User(form.ReceiverId.ToString()).SendAsync("UnreadCount", unreadCount);
        }

        public async Task MarkRead(Guid otherUserId)
        {
            var userId = GetUserId();
            if (userId == null || userId == Guid.Empty)
                return;

            await _messageServices.MarkConversationReadAsync(otherUserId, userId.Value);

            await Clients.User(otherUserId.ToString()).SendAsync("MessagesRead", userId.Value);
            await Clients.Caller.SendAsync("UnreadCount", 0);
        }

        public async Task Typing(Guid otherUserId)
        {
            var userId = GetUserId();
            if (userId == null || userId == Guid.Empty)
                return;

            await Clients.User(otherUserId.ToString()).SendAsync("UserTyping", userId.Value);
        }

        public async Task StopTyping(Guid otherUserId)
        {
            var userId = GetUserId();
            if (userId == null || userId == Guid.Empty)
                return;

            await Clients.User(otherUserId.ToString()).SendAsync("UserStopTyping", userId.Value);
        }
    }
}
