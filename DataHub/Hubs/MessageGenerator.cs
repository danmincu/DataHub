    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Timers;

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

    public class MessageGenerator: IDataGenerator
    {
        private static readonly Random RandomGenerator = new Random();
        private const int MaxStringLength = 10;        
        private readonly ISubject<string[]> _subject;
        private readonly System.Timers.Timer _timer;
        private int maxMessageCount = 5;
        private string? lastAckId;

        public MessageGenerator()
        {
            _subject = new ReplaySubject<string[]>();
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }
        
        public void SetAckId(string ackId) { 
            lastAckId = ackId;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (lastAckId != null)
            {
                return;
            }
            var randomStrings = new List<string>();
            var randomMessageCount = RandomGenerator.Next(0, this.maxMessageCount);
            for (var i = 0; i < Math.Max(0,Math.Min(randomMessageCount, 1000)); i++)
            {
                randomStrings.Add(GetRandomString());
            }
            if (randomStrings.Count > 0)
            {
                var newAckId = Guid.NewGuid().ToString();
                Console.WriteLine($"Sending data with ackId: {newAckId}");
                randomStrings.Add(newAckId);
            }
            _subject.OnNext(randomStrings.ToArray());
        }

        private static string GetRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, MaxStringLength)
                .Select(s => s[RandomGenerator.Next(s.Length)]).ToArray());
        }

        public IObservable<string[]> GetStream()
        {
            
            return _subject.AsObservable();
        }

        public void SetMaxMessageCount(int maxMessageCount)
        {
            this.maxMessageCount = maxMessageCount;
        }


        public void Reset()
        {
            lastAckId = null;
            Console.WriteLine("Reset");
        }

        public void Ack(string ackId)
        {
            if (lastAckId == ackId)
            {
                lastAckId = null;
                Console.WriteLine("AckId: " + ackId);
            }
        }
    }
}
