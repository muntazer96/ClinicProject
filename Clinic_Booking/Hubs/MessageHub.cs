using System.Security.Claims;
using Clinic_Booking.Data;
using Clinic_Booking.DTOs.MessageDTO;
using Clinic_Booking.Entities.Message;
using Clinic_Booking.IServices.IMessageServices;
using Clinic_Booking.Services.MessageServices;
using Clinic_Booking.Services.ProfanityFilterService;
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
                throw new HubException("You must be authenticated to send messages.");

            if (form.ReceiverId == userId.Value)
                throw new HubException("Cannot send a message to yourself.");

            var content = form.Content?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                throw new HubException("Message content cannot be empty.");

            if (ProfanityFilterServices.ContainsProfanity(content))
                throw new HubException("Message contains blocked words.");

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == form.ReceiverId);
            if (!receiverExists)
                throw new HubException("Receiver not found.");

            var canSend = await _messageServices.CanSendMessageAsync(userId.Value, form.ReceiverId);
            if (!canSend)
                throw new HubException("المراسلة متاحة فقط بين الطبيب والمراجع الذي أكمل مراجعته خلال آخر 3 أيام.");

            var message = new Message
            {
                SenderId = userId.Value,
                ReceiverId = form.ReceiverId,
                Content = content,
                SentAt = BusinessClock.Now(),
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

                var callerUnreadCount = await _messageServices.GetUnreadCountForUserAsync(userId.Value);
                await Clients.Caller.SendAsync("UnreadCount", callerUnreadCount);
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
            var unreadCount = await _messageServices.GetUnreadCountForUserAsync(userId.Value);

            try
            {
                await Clients.User(otherUserId.ToString()).SendAsync("MessagesRead", userId.Value);
                await Clients.Caller.SendAsync("UnreadCount", unreadCount);
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
