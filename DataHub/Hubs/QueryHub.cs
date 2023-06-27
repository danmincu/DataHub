using Microsoft.AspNetCore.SignalR;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DataHub.Hubs
{
    public class QueryHub : Hub
    {
        public static int TotalSuccesfullConnections { get; set; }

        private int maxLiveConnectionCount = 2;
        private int _maxMessageCount;

        private static readonly ConcurrentDictionary<string, string?> liveConnections = new ConcurrentDictionary<string, string?>();

        public async Task ConnectQuery(int maxMessageCount)
        {
            _maxMessageCount = maxMessageCount;
            TotalSuccesfullConnections++;
            await Clients.Group("liveConnections").SendAsync("updateTotalSuccesfullConnections", TotalSuccesfullConnections);
        }


        public async override Task OnConnectedAsync()
        {
            if (liveConnections.Count + 1 > maxLiveConnectionCount)
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "deadConnections");                
                await Clients.Client(Context.ConnectionId).SendAsync("maxLiveConnectionCountReached", maxLiveConnectionCount);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("connectionAccepted", maxLiveConnectionCount);
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "liveConnections");
                liveConnections.TryAdd(Context.ConnectionId, null);
                await Clients.Group("liveConnections").SendAsync("updateTotalLiveConnections", liveConnections.Count);
            }
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var cid = Context.ConnectionId;
            
            if (liveConnections.TryGetValue(cid, out string? value))
            {
                await this.Groups.RemoveFromGroupAsync(cid, "liveConnections").ConfigureAwait(false);
                liveConnections.TryRemove(cid, out string? value1);
            }
            else
            {
                await this.Groups.RemoveFromGroupAsync(cid, "deadConnections").ConfigureAwait(false);
            }
            
            Clients.Group("liveConnections").SendAsync("updateTotalLiveConnections", liveConnections.Count).GetAwaiter().GetResult();
            await base.OnDisconnectedAsync(exception);
        }

    }
}
