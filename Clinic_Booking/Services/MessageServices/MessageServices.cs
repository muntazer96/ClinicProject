using Clinic_Booking.Data;
using Clinic_Booking.DTOs.MessageDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Message;
using Clinic_Booking.Hubs;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IMessageServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.Services.NotificationDeliveryServices;
using Clinic_Booking.Services.ProfanityFilterService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.MessageServices
{
    public class MessageServices : IMessageServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IPushNotificationServices _pushNotificationServices;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly OnlineUserTracker _onlineTracker;

        public MessageServices(
            ApplicationDbContext context,
            ILoadServices load,
            IPushNotificationServices pushNotificationServices,
            IHubContext<MessageHub> hubContext,
            OnlineUserTracker onlineTracker)
        {
            _context = context;
            _load = load;
            _pushNotificationServices = pushNotificationServices;
            _hubContext = hubContext;
            _onlineTracker = onlineTracker;
        }

        public async Task<IActionResult> SendAsync(SendMessageDto form)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Unauthorized();

            if (form.ReceiverId == userId.Value)
                return BadRequest("Cannot send a message to yourself.");

            var content = form.Content?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Message content cannot be empty.");

            if (ProfanityFilterServices.ContainsProfanity(content))
                return BadRequest("Message contains blocked words.");

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == form.ReceiverId);
            if (!receiverExists)
                return NotFound("Receiver not found.");

            var canReceive = await ReceiverCanReceiveMessagesAsync(form.ReceiverId);
            if (!canReceive)
                return BadRequest("المستخدم لا يدعم خاصية الرسائل حالياً.");

            var message = new Message
            {
                SenderId = userId.Value,
                ReceiverId = form.ReceiverId,
                Content = content,
                SentAt = DateTime.UtcNow,
                Type = form.Type,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var dto = await MapToDtoAsync(message);

            await _hubContext.Clients.User(message.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", dto);

            var unreadCount = await GetUnreadCountForUserAsync(message.ReceiverId);
            await _hubContext.Clients.User(message.ReceiverId.ToString())
                .SendAsync("UnreadCount", unreadCount);

            if (!_onlineTracker.IsUserOnline(message.ReceiverId))
                await NotifyReceiverPushAsync(message);

            return Ok(dto, "Message sent successfully.");
        }

        public async Task<IActionResult> GetConversationsAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Unauthorized();

            var otherUserIds = await _context.Messages
                .Where(m => m.SenderId == userId.Value || m.ReceiverId == userId.Value)
                .Select(m => m.SenderId == userId.Value ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            if (otherUserIds.Count == 0)
                return Ok(new List<ConversationDto>(), "No conversations found.");

            var users = await _context.Users
                .Where(u => otherUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name, u.ImageName })
                .ToDictionaryAsync(u => u.Id);

            var conversations = new List<ConversationDto>();

            foreach (var otherId in otherUserIds)
            {
                if (!users.TryGetValue(otherId, out var user))
                    continue;

                var lastMessage = await _context.Messages
                    .Where(m => (m.SenderId == userId.Value && m.ReceiverId == otherId) ||
                                (m.SenderId == otherId && m.ReceiverId == userId.Value))
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new { m.Content, m.SentAt })
                    .FirstAsync();

                var unreadCount = await _context.Messages
                    .CountAsync(m => m.SenderId == otherId && m.ReceiverId == userId.Value && !m.IsRead);

                conversations.Add(new ConversationDto
                {
                    OtherUserId = otherId,
                    OtherUserName = user.Name ?? user.Id.ToString(),
                    OtherUserImage = user.ImageName,
                    LastMessage = lastMessage.Content,
                    LastMessageAt = lastMessage.SentAt,
                    UnreadCount = unreadCount
                });
            }

            return Ok(conversations.OrderByDescending(c => c.LastMessageAt).ToList(), "Conversations retrieved successfully.");
        }

        public async Task<IActionResult> GetConversationAsync(Guid otherUserId, int page = 1, int pageSize = 50)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Unauthorized();

            var query = _context.Messages
                .AsNoTracking()
                .Where(m => (m.SenderId == userId.Value && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId.Value))
                .OrderByDescending(m => m.SentAt);

            var totalCount = await query.CountAsync();

            var messages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<MessageDto>();
            foreach (var msg in messages)
                dtos.Add(await MapToDtoAsync(msg));

            return Ok(new
            {
                Messages = dtos.OrderBy(m => m.SentAt).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                HasMore = page * pageSize < totalCount
            }, "Messages retrieved successfully.");
        }

        public async Task<IActionResult> MarkAsReadAsync(Guid otherUserId)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Unauthorized();

            var now = DateTime.UtcNow;
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId.Value && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Count == 0)
                return Ok(new { MarkedRead = 0 }, "No unread messages.");

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                msg.ReadAt = now;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(otherUserId.ToString())
                .SendAsync("MessagesRead", userId.Value, unreadMessages.Count);

            var unreadCount = await GetUnreadCountForUserAsync(userId.Value);
            await _hubContext.Clients.User(userId.Value.ToString())
                .SendAsync("UnreadCount", unreadCount);

            return Ok(new { MarkedRead = unreadMessages.Count, UnreadCount = unreadCount }, "Messages marked as read.");
        }

        public async Task<IActionResult> GetUnreadCountAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Unauthorized();

            var count = await _context.Messages
                .CountAsync(m => m.ReceiverId == userId.Value && !m.IsRead);

            return Ok(new { UnreadCount = count }, "Unread count retrieved successfully.");
        }

        public async Task<Entities.Message.Message> SaveAndReturnAsync(Entities.Message.Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task MarkConversationReadAsync(Guid senderId, Guid receiverId)
        {
            var now = DateTime.UtcNow;
            var unread = await _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unread)
            {
                msg.IsRead = true;
                msg.ReadAt = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountForUserAsync(Guid userId)
        {
            return await _context.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        public async Task<MessageDto?> GetMessageDtoAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            return message == null ? null : await MapToDtoAsync(message);
        }

        private async Task NotifyReceiverPushAsync(Entities.Message.Message message)
        {
            try
            {
                var senderName = await _context.Users
                    .Where(u => u.Id == message.SenderId)
                    .Select(u => u.Name ?? "User")
                    .FirstAsync();

                var title = "رسالة جديدة";
                var body = $"لديك رسالة جديدة من {senderName}";
                var data = new Dictionary<string, string>
                {
                    ["type"] = "new_message",
                    ["senderId"] = message.SenderId.ToString(),
                    ["messageId"] = message.Id.ToString()
                };

                var sent = await _pushNotificationServices.SendToUserAsync(
                    message.ReceiverId, title, body, data);

                NotificationDeliveryAttemptRecorder.AddPushAttempt(
                    _context, sent, message.ReceiverId, title, body, data);
                await _context.SaveChangesAsync();
            }
            catch
            {
            }
        }

        public async Task<bool> ReceiverCanReceiveMessagesAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var doctor = await _context.Doctors
                .Include(d => d.DoctorSubscriptions).ThenInclude(s => s.Package)
                .Include(d => d.DoctorFeatures).ThenInclude(f => f.Feature)
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return true;

            var hasSubscription = doctor.DoctorSubscriptions.Any(s =>
                s.Status == Enums.SubscriptionStatus.Active &&
                s.StartDate <= now &&
                s.EndDate >= now &&
                s.Package.ShowMessages);

            var hasFeature = doctor.DoctorFeatures.Any(f =>
                !f.IsDeleted &&
                f.IsEnabled &&
                f.Feature.NormalizedName == "ShowMessages");

            return hasSubscription && hasFeature;
        }

        private async Task<MessageDto> MapToDtoAsync(Entities.Message.Message message)
        {
            var sender = await _context.Users.FindAsync(message.SenderId);
            var receiver = await _context.Users.FindAsync(message.ReceiverId);

            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = sender?.Name ?? "Unknown",
                SenderImage = sender?.ImageName,
                ReceiverId = message.ReceiverId,
                ReceiverName = receiver?.Name ?? "Unknown",
                ReceiverImage = receiver?.ImageName,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                ReadAt = message.ReadAt,
                Type = message.Type
            };
        }

        private static IActionResult Ok<T>(T data, string message)
        {
            return new OkObjectResult(new ResponseDto<T>
            {
                Status = "Success",
                Code = 200,
                Message = message,
                Data = data
            });
        }

        private static IActionResult BadRequest(string message)
        {
            return new BadRequestObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 400,
                Message = message
            });
        }

        private static IActionResult NotFound(string message)
        {
            return new NotFoundObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 404,
                Message = message
            });
        }

        private static IActionResult Unauthorized()
        {
            return new UnauthorizedObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 401,
                Message = "You must sign in to use messaging."
            });
        }
    }
}
