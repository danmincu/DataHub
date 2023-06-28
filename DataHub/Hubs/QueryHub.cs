using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Hub = Microsoft.AspNetCore.SignalR.Hub;

namespace DataHub.Hubs
{
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "Bearer")]
    //[Microsoft.AspNet.SignalR.Authorize(AuthenticationSchemes = "Bearer")]
    public class QueryHub : Hub
    {
        public static int TotalSuccesfullConnections { get; set; }
        
        public QueryHub(IDataGenerator dataGenerator)
        {
            this._dataGenerator = dataGenerator;
        }

        private int maxLiveConnectionCount = 2;
        private int _maxMessageCount;
        private IDisposable? _subscription;

        protected override void Dispose(bool disposing)
        {
            _subscription?.Dispose();
            base.Dispose(disposing);
        }

        private static readonly ConcurrentDictionary<string, string?> liveConnections = new ConcurrentDictionary<string, string?>();
        private readonly IDataGenerator _dataGenerator;

        public async Task<string> ConnectQuery(int maxMessageCount, string? ackId = null)
        {            
            _maxMessageCount = maxMessageCount;
            //Context.Items.Add("maxMessageCount", maxMessageCount);
            this._dataGenerator.SetMaxMessageCount(maxMessageCount);
            if (ackId != null)
            {
                await this.Ack(ackId);
            }
            TotalSuccesfullConnections++;
            await Clients.Group("liveConnections").SendAsync("updateTotalSuccesfullConnections", TotalSuccesfullConnections);
            return Context.ConnectionId;
        }


        public async Task<string> ReceivedAck(string ackId)
        {
            Console.WriteLine("Confirming the delivery of data with AckId: " + ackId);
            this._dataGenerator.SetAckId(ackId);
            return await Task.FromResult(ackId);
        }

        public async Task<string> Ack(string ackId)
        {
            this._dataGenerator.Ack(ackId);
            return await Task.FromResult(ackId);
        }

        //public async Task data(string ackId, string data)
        //{
        //    Console.WriteLine($"Sending data with ackId: {ackId}");
        //    await Clients.Group("liveConnections").SendAsync("data", ackId, data);
        //}

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
