using System;
using Consolation.Common.Framework.OptionsSystem;

namespace Consolation.Common
{
    public abstract class ConsoleWindow
    {
        public abstract ConsoleOptions DefaultOptions { get; }

        public abstract void WriteStaticText(bool withMessage);

        public virtual void Clear(bool withMessage)
        {
            Console.Clear();
            WriteStaticText(withMessage);
        }

        public virtual void WriteAndClear(string message, ConsoleColor color = ConsoleColor.Red)
        {
            Clear(true);
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public virtual void WriteOptionsList(ConsoleOptions options)
        {
            ConsoleAPI.SelectedOptionSet = options;
            ConsoleAPI.SelectedOptionSet.ListForOption();
        }
    }
}
