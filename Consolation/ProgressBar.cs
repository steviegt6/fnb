using System;
using System.Text;
using System.Threading;
using Consolation.Utilities;

namespace Consolation
{
    /// <summary>
    ///     Simple disposable class for representing a progress bar in the console window.
    /// </summary>
    public class ProgressBar : IDisposable, IProgress<int>
    {
        private int _currentElements;

        /// <summary>
        ///     Construct a new <see cref="ProgressBar"/> instance with a <see cref="ConsoleWindow"/> parameter and an adjustable bar size (<see cref="barSize"/>).
        /// </summary>
        public ProgressBar(ConsoleWindow? window = null, byte barSize = 16)
        {
            Window = window;
            CurrentElements = 0;
            Timer = new Timer(TimerHandle);
            NumberOfBlocks = barSize;
        }

        /// <summary>
        ///     Used <see cref="ConsoleWindow"/> instance. Uses a regular <see cref="Console"/> if no instance is provided.
        /// </summary>
        public ConsoleWindow? Window { get; protected set; }

        // Original field kept due to ref requirements
        /// <summary>
        ///     Current progress elements.
        /// </summary>
        public int CurrentElements
        {
            get => _currentElements;
            protected set => _currentElements = value;
        }

        /// <summary>
        ///     Configured progress bar length.
        /// </summary>
        public byte NumberOfBlocks { get; protected set; }

        /// <summary>
        ///     How often the progress bar is updated per second.
        /// </summary>
        public virtual TimeSpan AnimationInterval => TimeSpan.FromSeconds(1.0D / 8D); // Update 8 times a second 

        /// <summary>
        ///     The maximum amount of elements in the progress bar.
        /// </summary>
        public virtual int? MaxElements { get; protected set; }

        /// <summary>
        ///     Animation timer. Also used for <code>lock</code> blocks.
        /// </summary>
        public Timer Timer { get; protected set; }

        /// <summary>
        ///     Whether or not this object is disposed.
        /// </summary>
        public bool Disposed { get; protected set; }

        /// <summary>
        ///     Disposes this object.
        /// </summary>
        public virtual void Dispose()
        {
            lock (Timer) 
                Disposed = true;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Report (update) the progress bar with the given amount.
        /// </summary>
        /// <param name="amount"></param>
        public virtual void Report(int amount)
        {
            if (!MaxElements.HasValue)
            {
                MaxElements = amount;
                return;
            }

            Interlocked.Add(ref _currentElements, amount);
        }

        /// <summary>
        ///     Resets the timer.
        /// </summary>
        public virtual void Start()
        {
            ResetTimer();
        }

        /// <summary>
        ///     Finalizes the progress text, disposes the instance, and writes a line to the console beneath it.
        /// </summary>
        public virtual void Finish()
        {
            UpdateProgressText(CreateProgressText());
            Dispose();

            WindowUtilities.ExecuteWithConsole(window => window.WriteLine(), Console.WriteLine, Window);
        }

        /// <summary>
        ///     The handle used for updating visual progress text. Attached to the <see cref="Timer"/>.
        /// </summary>
        protected virtual void TimerHandle(object? state)
        {
            lock (Timer)
            {
                if (Disposed) return;

                ResetTimer();
                UpdateProgressText(CreateProgressText());
            }
        }

        /// <summary>
        ///     Resets the <see cref="Timer"/> used for the <see cref="AnimationInterval"/>.
        /// </summary>
        protected virtual void ResetTimer()
        {
            Timer.Change(AnimationInterval, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        ///     Creates a string representing the current progress text.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Visually updates the progress text.
        /// </summary>
        /// <param name="text"></param>
        protected virtual void UpdateProgressText(string text)
        {
            WindowUtilities.ExecuteWithConsole(window => window.Write(text), () => Console.Write(text), Window);
        }

        /// <summary>
        ///     Creates and starts a new <see cref="ProgressBar"/> instance.
        /// </summary>
        /// <returns>A <see cref="Start"/>ed <see cref="ProgressBar"/>.</returns>
        public static ProgressBar StartNew(ConsoleWindow? window = null, byte barSize = 16)
        {
            ProgressBar bar = new(window, barSize);
            bar.Start();
            return bar;
        }
    }
}