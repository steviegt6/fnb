using System;
using System.Text;
using System.Threading;

namespace Consolation.Common
{
    public class ProgressBar : IDisposable, IProgress<int>
    {
        private int _currentElements;

        public ProgressBar(ConsoleWindow? window = null, byte barSize = 16)
        {
            Window = window;
            CurrentElements = 0;
            Timer = new Timer(TimerHandle);
            NumberOfBlocks = barSize;
        }

        public ConsoleWindow? Window { get; protected set; }

        // Original field kept due to ref requirements
        public int CurrentElements
        {
            get => _currentElements;
            protected set => _currentElements = value;
        }

        public byte NumberOfBlocks { get; protected set; }

        public virtual TimeSpan AnimationInterval => TimeSpan.FromSeconds(1.0 / 8); // Update 8 times a second 

        public virtual int? MaxElements { get; protected set; }

        public Timer Timer { get; protected set; }

        public bool Disposed { get; protected set; }

        public virtual void Dispose()
        {
            lock (Timer)
            {
                Disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public virtual void Report(int amount)
        {
            if (!MaxElements.HasValue)
            {
                MaxElements = amount;
                return;
            }

            Interlocked.Add(ref _currentElements, amount);
        }

        public static ProgressBar StartNew(ConsoleWindow? window = null, byte barSize = 16)
        {
            ProgressBar bar = new(window, barSize);
            bar.Start();
            return bar;
        }

        public virtual void Start()
        {
            ResetTimer();
        }

        public virtual void Finish()
        {
            UpdateProgressText(CreateProgressText());
            Dispose();
            Window?.WriteLine();
        }

        protected virtual void TimerHandle(object? state)
        {
            lock (Timer)
            {
                if (Disposed)
                    return;

                ResetTimer();
                UpdateProgressText(CreateProgressText());
            }
        }

        protected virtual void ResetTimer()
        {
            Timer.Change(AnimationInterval, TimeSpan.FromMilliseconds(-1));
        }

        protected virtual string CreateProgressText()
        {
            if (!MaxElements.HasValue)
                return "";

            double percent = (double) CurrentElements / MaxElements.Value;
            int numFullBlocks = (int) Math.Round(percent * NumberOfBlocks);
            StringBuilder sb = new();

            for (int i = 0; i < NumberOfBlocks; i++)
                sb.Append(i <= numFullBlocks ? "#" : "-");

            return $"\r[{sb}] {CurrentElements}/{MaxElements}";
        }

        protected virtual void UpdateProgressText(string text)
        {
            Console.Write(text);
        }
    }
}