namespace DataHub.Hubs
{
    public interface IDataGenerator
    {
        IObservable<string[]> GetStream();

        void SetMaxMessageCount(int maxMessageCount);

        void Ack(string ackId);

        void SetAckId(string ackId);

        void Reset();
    }
}
