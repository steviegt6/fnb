namespace Consolation.Framework.OptionsSystem
{
    /// <summary>
    ///     Simple console option.
    /// </summary>
    public abstract class ConsoleOption
    {
        /// <summary>
        ///     Index in a <see cref="ConsoleOptions"/> list.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     Display text.
        /// </summary>
        public abstract string Text { get; }

        /// <summary>
        ///     Method called during execution.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        ///     Returns text showing the position in the <see cref="ConsoleOptions"/> list as well as display text (<see cref="Text"/>).
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString() => $"  [{Index + 1}] {Text}";
    }
}