using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace PropertyLockingSystem.Hubs
{
    public class PropertyLockHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> PropertyLocks = new();

        public async Task LockProperty(string propertyId, string userName)
        {
            // Attempt atomic lock acquisition without a separate ContainsKey check to avoid race conditions.
            // TryAdd returns true only if the key did not exist and was added.
            if (PropertyLocks.TryAdd(propertyId, Context.ConnectionId))
            {
                await Clients.Caller.SendAsync("LockAcquired", propertyId);
                await Clients.Others.SendAsync("PropertyLocked", propertyId, userName);
            }
            else
            {
                await Clients.Caller.SendAsync("LockDenied", propertyId);
            }
        }

        public async Task UnlockProperty(string propertyId)
        {
            // Try to get the owner of the lock to ensure ownership before unlocking.
            if (PropertyLocks.TryGetValue(propertyId, out var ownerConnectionId))
            {
                // Only the connection that owns the lock can unlock it.
                if (ownerConnectionId == Context.ConnectionId)
                {
                    if (PropertyLocks.TryRemove(propertyId, out _))
                    {
                        await Clients.All.SendAsync("PropertyUnlocked", propertyId);
                    }
                }
                else 
                {
                    //Inform caller they don't own the lock.
                    await Clients.Caller.SendAsync("UnlockDenied", propertyId, "You do not own this lock.");
                }
            }
            else
            {
                //Inform caller the property was not locked.
                await Clients.Caller.SendAsync("UnlockDenied", propertyId, "Property is not locked.");
            }
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            // Find all properties locked by this connection and remove them.
            var lockedEntries = PropertyLocks.Where(x => x.Value == Context.ConnectionId).ToList();

            foreach (var lockedEntry in lockedEntries)
            {
                if (!string.IsNullOrEmpty(lockedEntry.Key))
                {
                    PropertyLocks.TryRemove(lockedEntry.Key, out _);
                    await Clients.All.SendAsync("PropertyUnlocked", lockedEntry.Key);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
