namespace DataHubClientConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            SignalRClient client = new SignalRClient();

            // Connect to SignalR Hub
            Console.WriteLine("CreateSignalRConnection...");
            client.CreateSignalRConnection();
            Console.WriteLine("Press any key to disconnect...");
            Console.ReadKey();
            client.DisconnectConnection();
        }
    }
}