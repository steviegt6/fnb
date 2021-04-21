using System;
using System.Text;
using System.Threading;

namespace Consolation.Common
{
    public class ProgressBar : IDisposable, IProgress<int>
    {
        protected virtual byte NumberOfBlocks => 16;
        protected virtual TimeSpan AnimationInterval => TimeSpan.FromSeconds(1.0 / 8); // Update 8 times a second 
        
        private int? _maxElements;
        private int _currentElements;
        private readonly Timer _timer;
        private bool _disposed;
        
        public ProgressBar()
        {
            _currentElements = 0;
            _timer = new Timer(TimerHandle);
        }

        public void Report(int amount)
        {
            if (!_maxElements.HasValue)
            {
                _maxElements = amount;
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
            lock (_timer) _disposed = true;
            
            GC.SuppressFinalize(this);
        }
        
        protected virtual void TimerHandle(object state)
        {
            lock (_timer)
            {
                if (_disposed) return;

                ResetTimer();
                UpdateProgressText(CreateProgressText());
            }
        }

        protected virtual void ResetTimer() => _timer.Change(AnimationInterval, TimeSpan.FromMilliseconds(-1));

        protected virtual string CreateProgressText()
        {
            if (!_maxElements.HasValue)
                return "";
            
            double percent = (double)_currentElements / _maxElements.Value;

            int numFullBlocks = (int) Math.Round(percent * NumberOfBlocks);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NumberOfBlocks; i++) sb.Append(i <= numFullBlocks ? "#" : "-");

            return $"\r[{sb}] {_currentElements}/{_maxElements}";
        }
        
        protected virtual void UpdateProgressText(string text) => Console.Write(text);
    }
}