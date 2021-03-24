using TMLPatcher.Common.Framework;

namespace TMLPatcher.Common.Options
{
    public class TestOption : ConsoleOption
    {
        public override string Name => "TEST";

        public override string Text => "Open new menu.";

        public override void Execute()
        {
            Program.Clear(false);
            new ConsoleOptions("Sub-menu!", new TestOption2()).ListForOption();
        }
    }
}
