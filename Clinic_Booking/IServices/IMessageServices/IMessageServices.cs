using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IMessageServices
{
    public interface IMessageServices
    {
        Task<IActionResult> SendAsync(DTOs.MessageDTO.SendMessageDto form);
        Task<IActionResult> GetConversationsAsync();
        Task<IActionResult> GetConversationAsync(Guid otherUserId, int page = 1, int pageSize = 50);
        Task<IActionResult> MarkAsReadAsync(Guid otherUserId);
        Task<IActionResult> GetUnreadCountAsync();

        Task<Entities.Message.Message> SaveAndReturnAsync(Entities.Message.Message message);
        Task<DTOs.MessageDTO.MessageDto?> GetMessageDtoAsync(int messageId);
        Task<int> GetUnreadCountForUserAsync(Guid userId);
        Task MarkConversationReadAsync(Guid senderId, Guid receiverId);
        Task<bool> ReceiverCanReceiveMessagesAsync(Guid userId);
    }
}
