using System;

namespace TML.Patcher
{
    public class ActionableProgress : ActionableProgress<ProgressNotification>
    {
    }

    public class ActionableProgress<T> : IProgress<T>
    {
        public event Action<T>? OnReport;

        public void Report(T value) => OnReport?.Invoke(value);
    }
}