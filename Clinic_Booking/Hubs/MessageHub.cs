using System.Security.Claims;
using Clinic_Booking.Data;
using Clinic_Booking.DTOs.MessageDTO;
using Clinic_Booking.Entities.Message;
using Clinic_Booking.IServices.IMessageServices;
using Clinic_Booking.Services.MessageServices;
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
        private readonly OnlineUserTracker _onlineTracker;

        public MessageHub(
            IMessageServices messageServices,
            ApplicationDbContext context,
            OnlineUserTracker onlineTracker)
        {
            _messageServices = messageServices;
            _context = context;
            _onlineTracker = onlineTracker;
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
            try
            {
                var userId = GetUserId();
                if (userId.HasValue && userId.Value != Guid.Empty)
                {
                    _onlineTracker.UserConnected(userId.Value);
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");

                    var unreadCount = await _messageServices.GetUnreadCountForUserAsync(userId.Value);
                    await Clients.Caller.SendAsync("UnreadCount", unreadCount);
                }
            }
            catch
            {
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = GetUserId();
                if (userId.HasValue && userId.Value != Guid.Empty)
                {
                    _onlineTracker.UserDisconnected(userId.Value);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
                }
            }
            catch
            {
            }

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

            var canReceive = await _messageServices.ReceiverCanReceiveMessagesAsync(form.ReceiverId);
            if (!canReceive)
            {
                await Clients.Caller.SendAsync("Error", "المستخدم لا يدعم خاصية الرسائل حالياً.");
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

            try
            {
                await Clients.Caller.SendAsync("MessageSent", dto);
                await Clients.User(form.ReceiverId.ToString()).SendAsync("ReceiveMessage", dto);

                var unreadCount = await _messageServices.GetUnreadCountForUserAsync(form.ReceiverId);
                await Clients.User(form.ReceiverId.ToString()).SendAsync("UnreadCount", unreadCount);
            }
            catch
            {
            }
        }

        public async Task MarkRead(Guid otherUserId)
        {
            var userId = GetUserId();
            if (userId == null || userId == Guid.Empty)
                return;

            await _messageServices.MarkConversationReadAsync(otherUserId, userId.Value);

            try
            {
                await Clients.User(otherUserId.ToString()).SendAsync("MessagesRead", userId.Value);
                await Clients.Caller.SendAsync("UnreadCount", 0);
            }
            catch
            {
            }
        }

        public async Task Typing(Guid otherUserId)
        {
            var userId = GetUserId();
            if (userId == null || userId == Guid.Empty)
                return;

            try
            {
                await Clients.User(otherUserId.ToString()).SendAsync("UserTyping", userId.Value);
            }
            catch
            {
            }
        }

        public async Task StopTyping(Guid otherUserId)
        {
            var userId = GetUserId();
            if (userId == null || userId == Guid.Empty)
                return;

            try
            {
                await Clients.User(otherUserId.ToString()).SendAsync("UserStopTyping", userId.Value);
            }
            catch
            {
            }
        }
    }
}
