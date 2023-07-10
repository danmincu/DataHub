using Microsoft.AspNetCore.SignalR.Client;

namespace DataHubClientConsole
{
    public class SignalRClient
    {
        private HubConnection connection;

        private async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://localhost:7016/security/createToken?user=service-account-data");

                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    return contents.Replace("\"","");
                }
                else
                {
                    // Handle error
                    return null;
                }
            }
        }

        public async void CreateSignalRConnection()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7016/hubs/query", options =>
                {
                    options.AccessTokenProvider = async () => await GetToken().ConfigureAwait(false);
                })
                .WithAutomaticReconnect()
                .Build();


            connection.On<int>("updateTotalSuccesfullConnections", (count) =>
            {
                Console.WriteLine($"Total successful connections: {count}");
            });

            connection.On<string>("updateTotalLiveConnections", (query) =>
            {
                Console.WriteLine($"Total live connections: {query}");
            });

            connection.On<string, string>("data", (ackid, data) =>
            {
                Console.WriteLine("---------------------------");
                Console.WriteLine($"AckId: {ackid}");

                Console.WriteLine($"{data}");
            });

            try
            {
                await connection.StartAsync();
                await connection.SendAsync("connectToHub", 15, "some-ACK_ID");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async void DisconnectConnection()
        {
            await connection.StopAsync();
            connection = null;
        }
    }
}
