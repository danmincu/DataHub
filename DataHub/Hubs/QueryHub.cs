using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DataHub.Hubs
{
    public class QueryHub : Hub, IHub
    {
        public static int TotalSuccesfullConnections { get; set; }
        Microsoft.AspNet.SignalR.Hubs.HubCallerContext IHub.Context { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IHubCallerConnectionContext<dynamic> IHub.Clients { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Microsoft.AspNet.SignalR.IGroupManager IHub.Groups { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private int maxLiveConnectionCount = 2;
        private int _maxMessageCount;
        private IDisposable? _subscription;

        public QueryHub()
        {
            _subscription = Program.SimulatedKakaQueue.Subscribe(async (message) =>
            {
                //if (liveConnections.Count > 0)
                //{
                //    await this.data(Guid.NewGuid().ToString(), string.Join(',',message));
                //}
            });
        }

        protected override void Dispose(bool disposing)
        {
            _subscription?.Dispose();
            base.Dispose(disposing);
        }

        private static readonly ConcurrentDictionary<string, string?> liveConnections = new ConcurrentDictionary<string, string?>();

        public async Task<string> ConnectQuery(int maxMessageCount, string? ackId = null)
        {            
            _maxMessageCount = maxMessageCount;
            if (ackId != null)
            {
                await this.Ack(ackId);
            }
            TotalSuccesfullConnections++;
            await Clients.Group("liveConnections").SendAsync("updateTotalSuccesfullConnections", TotalSuccesfullConnections);
            return Context.ConnectionId;
        }

        public async Task<string> Ack(string ackId)
        {
            Console.WriteLine("AckId: " + ackId);
            return await Task.FromResult(ackId);
        }

        public async Task data(string ackId, string data)
        {
            Console.WriteLine($"Sending data with ackId: {ackId}");
            await Clients.Group("liveConnections").SendAsync("data", ackId, data);
        }

        public async override Task OnConnectedAsync()
        {
            Console.WriteLine("Connect " + Context.ConnectionId);

            if (liveConnections.Count + 1 > maxLiveConnectionCount)
            {
                await Clients.Caller.SendAsync("maxLiveConnectionCountReached", maxLiveConnectionCount);
            }
            else
            {
                
                await Clients.Client(Context.ConnectionId).SendAsync("connectionAccepted", maxLiveConnectionCount);
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "liveConnections");
                liveConnections.TryAdd(Context.ConnectionId, null);
                await Clients.Group("liveConnections").SendAsync("updateTotalLiveConnections", liveConnections.Count);
                await base.OnConnectedAsync();
                _subscription = Program.SimulatedKakaQueue.Subscribe(async (message) =>
                {
                    await Clients.Group("liveConnections").SendAsync("data", Guid.NewGuid(), message);
                });
            }
            
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var cid = Context.ConnectionId;
            Console.WriteLine("Disconnect " + cid);

            if (liveConnections.TryGetValue(cid, out string? value))
            {
                await this.Groups.RemoveFromGroupAsync(cid, "liveConnections").ConfigureAwait(false);
                liveConnections.TryRemove(cid, out string? value1);
            }
            
            Clients.Group("liveConnections").SendAsync("updateTotalLiveConnections", liveConnections.Count).GetAwaiter().GetResult();
            await base.OnDisconnectedAsync(exception);
        }

        public Task OnConnected()
        {
            throw new NotImplementedException();
        }

        public Task OnReconnected()
        {
            throw new NotImplementedException();
        }

        public Task OnDisconnected(bool stopCalled)
        {
            throw new NotImplementedException();
        }
    }
}
