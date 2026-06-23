using System.Collections.Concurrent;

namespace Clinic_Booking.Services.MessageServices
{
    public class OnlineUserTracker
    {
        private readonly ConcurrentDictionary<Guid, int> _connections = new();

        public void UserConnected(Guid userId)
        {
            _connections.AddOrUpdate(userId, 1, (_, count) => count + 1);
        }

        public void UserDisconnected(Guid userId)
        {
            _connections.AddOrUpdate(userId,
                _ => 0,
                (_, count) =>
                {
                    var newCount = count - 1;
                    return newCount <= 0 ? 0 : newCount;
                });

            if (_connections.TryGetValue(userId, out var count) && count == 0)
            {
                _connections.TryRemove(userId, out _);
            }
        }

        public bool IsUserOnline(Guid userId) => _connections.ContainsKey(userId);
    }
}
