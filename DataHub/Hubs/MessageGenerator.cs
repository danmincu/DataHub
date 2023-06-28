    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Timers;

namespace DataHub.Hubs
{ 

    public class ObservableArraySubject
    {
        private static readonly Random RandomGenerator = new Random();
        private const int MaxStringLength = 10;
        private const int MaxArrayLength = 5;
        private readonly ISubject<string[]> _subject;
        private readonly System.Timers.Timer _timer;

        public ObservableArraySubject()
        {
            _subject = new ReplaySubject<string[]>();
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += TimerElapsed;

            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var randomStrings = new List<string>();
            for (var i = 0; i < MaxArrayLength; i++)
            {
                randomStrings.Add(GetRandomString());
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
    }
}
