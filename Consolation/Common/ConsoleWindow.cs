using System;
using System.Text;
using Consolation.Common.Framework.OptionsSystem;

namespace Consolation.Common
{
    public abstract class ConsoleWindow
    {
        public abstract ConsoleOptions DefaultOptions { get; }

        public virtual int SpaceCount { get; set; }

        public abstract void WriteStaticText(bool withMessage);

        public virtual void WriteLine() => Console.WriteLine();

        public virtual void WriteLine(int spaces, string message)
        {
            SpaceCount = spaces;
            WriteLine(message);
        }

        public virtual void WriteLine(string message)
        {
            StringBuilder sb = new();

            if (SpaceCount > 0)
                for (int i = 0; i < SpaceCount; i++)
                    sb.Append(' ');

            Console.WriteLine(sb.Append(message));
        }

        public virtual void Clear(bool withMessage)
        {
            Console.Clear();
            WriteStaticText(withMessage);
        }

        public virtual void WriteAndClear(string message, ConsoleColor color = ConsoleColor.Red)
        {
            SpaceCount = 0;
            Clear(true);
            Console.ForegroundColor = color;
            WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public virtual void WriteOptionsList(ConsoleOptions options)
        {
            SpaceCount = 0;
            ConsoleAPI.SelectedOptionSet = options;
            ConsoleAPI.SelectedOptionSet.ListForOption();
        }
    }
}
