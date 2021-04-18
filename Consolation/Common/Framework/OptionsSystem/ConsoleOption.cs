namespace Consolation.Common.Framework.OptionsSystem
{
    public abstract class ConsoleOption
    {
        public int Index { get; set; }

        public abstract string Text { get; }

        public abstract void Execute();

        public sealed override string ToString() => $"  [{Index + 1}] {Text}";
    }
}
