using System;
using System.Text;
using System.Threading;

namespace Consolation.Common
{
    public class ProgressBar : IDisposable, IProgress<int>
    {
        private int _currentElements;

        public virtual byte NumberOfBlocks => 16;

        public virtual TimeSpan AnimationInterval => TimeSpan.FromSeconds(1.0 / 8); // Update 8 times a second 

        public int? MaxElements { get; protected set; }

        public int CurrentElements
        {
            get => _currentElements;
            protected set => _currentElements = value;
        }

        public Timer Timer { get; protected set; }

        public bool Disposed { get; protected set; }

        public ProgressBar()
        {
            CurrentElements = 0;
            Timer = new Timer(TimerHandle);
        }

        public static ProgressBar StartNew()
        {
            ProgressBar bar = new ProgressBar();
            bar.Start();
            return bar;
        }

        public void Report(int amount)
        {
            if (!MaxElements.HasValue)
            {
                MaxElements = amount;
                return;
            }

            Interlocked.Add(ref _currentElements, amount);
        }

        public virtual void Start() => ResetTimer();

        public virtual void Finish()
        {
            UpdateProgressText(CreateProgressText());
            Dispose();
            Console.WriteLine();
        }

        public void Dispose()
        {
            lock (Timer) Disposed = true;

            GC.SuppressFinalize(this);
        }

        protected virtual void TimerHandle(object state)
        {
            lock (Timer)
            {
                if (Disposed) return;

                ResetTimer();
                UpdateProgressText(CreateProgressText());
            }
        }

        protected virtual void ResetTimer() => Timer.Change(AnimationInterval, TimeSpan.FromMilliseconds(-1));

        protected virtual string CreateProgressText()
        {
            if (!MaxElements.HasValue)
                return "";

            double percent = (double) CurrentElements / MaxElements.Value;

            int numFullBlocks = (int) Math.Round(percent * NumberOfBlocks);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NumberOfBlocks; i++) sb.Append(i <= numFullBlocks ? "#" : "-");

            return $"\r[{sb}] {CurrentElements}/{MaxElements}";
        }

        protected virtual void UpdateProgressText(string text) => Console.Write(text);
    }
}