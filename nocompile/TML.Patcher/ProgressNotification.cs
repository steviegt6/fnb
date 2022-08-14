namespace TML.Patcher
{
    public readonly struct ProgressNotification
    {
        /// <summary>
        ///     Current number of items complete.
        /// </summary>
        public readonly int Current;
        
        /// <summary>
        ///     Total number of items to complete.
        /// </summary>
        public readonly int Total;
        
        /// <summary>
        ///     A textual status update.
        /// </summary>
        public readonly string Status;
        
        /// <summary>
        ///     Whether there are any items to complete. If not, <see cref="Current"/> and <see cref="Total"/> are ignored.
        /// </summary>
        public readonly bool Progressive;

        public ProgressNotification(string status, int total, int current)
        {
            Status = status;
            Total = total;
            Current = current;
            Progressive = true;
        }

        public ProgressNotification(string status)
        {
            Status = status;
            Total = 1;
            Current = 1;
            Progressive = false;
        }

        public static implicit operator ProgressNotification(string value) => new(value);
    }
}