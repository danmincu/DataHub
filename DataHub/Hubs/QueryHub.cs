using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Hub = Microsoft.AspNetCore.SignalR.Hub;

namespace DataHub.Hubs
{
    // if AuthenticationSchemes = "Bearer" is missing it behaves like an AllowAnonymous Hub. didn't investigate why.
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "service")]    
    public class QueryHub : Hub
    {
        public static int TotalSuccesfullConnections { get; set; }
        
        public QueryHub(IDataGenerator dataGenerator)
        {
            this._dataGenerator = dataGenerator;
        }

        private int maxLiveConnectionCount = 2;
        private IDisposable? _subscription;

        protected override void Dispose(bool disposing)
        {
            _subscription?.Dispose();
            base.Dispose(disposing);
        }

        private static readonly ConcurrentDictionary<string, string?> liveConnections = new ConcurrentDictionary<string, string?>();
        private readonly IDataGenerator _dataGenerator;

        [Microsoft.AspNetCore.SignalR.HubMethodName("connectToHub")]
        public async Task<string> ConnectQuery(int maxMessageCount, string? ackId = null)
        {            
            this._dataGenerator.SetMaxMessageCount(maxMessageCount);
            if (ackId != null)
            {
                await this.Ack(ackId);
            }
            TotalSuccesfullConnections++;
            await Clients.Group("liveConnections").SendAsync("updateTotalSuccesfullConnections", TotalSuccesfullConnections);
            return Context.ConnectionId;
        }


        [Microsoft.AspNetCore.SignalR.HubMethodName("soft-ack")]
        public async Task<string> ReceivedAck(string ackId)
        {
            Console.WriteLine("Confirming the delivery of data with AckId: " + ackId);
            this._dataGenerator.SetAckId(ackId);
            return await Task.FromResult(ackId);
        }


        // THIS

        [Microsoft.AspNetCore.SignalR.HubMethodName("hard-ack")]
        public async Task<string> Ack(string ackId)
        {
            this._dataGenerator.Ack(ackId);
            return await Task.FromResult(ackId);
        }


        // THIS
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
            }
            
        }


        // THIS

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var cid = Context.ConnectionId;
            Console.WriteLine("Disconnect " + cid);

            if (liveConnections.TryGetValue(cid, out string? value))
            {
                await this.Groups.RemoveFromGroupAsync(cid, "liveConnections").ConfigureAwait(false);
                liveConnections.TryRemove(cid, out string? value1);
                if (liveConnections.Count == 0)
                {
                    this._dataGenerator.Reset();
                }
            }
            
            Clients.Group("liveConnections").SendAsync("updateTotalLiveConnections", liveConnections.Count).GetAwaiter().GetResult();
            await base.OnDisconnectedAsync(exception);
        }
    }
}
