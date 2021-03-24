using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TMLPatcher.Common.Framework
{
    public class ConsoleOptions : ICloneable, IList<ConsoleOption>
    {
        #region ConsoleOptions Code

        public ConsoleOption this[int index]
        {
            get => _options[index];

            set => _options[index] = value;
        }

        public string OptionText { get; }

        private readonly List<ConsoleOption> _options;

        private readonly ConsoleOptions _prevOptionsState;

        public ConsoleOptions(string optionText, ConsoleOptions prevOptionsState, params ConsoleOption[] options)
        {
            OptionText = optionText;
            _prevOptionsState = prevOptionsState;

            // Assign index values to the ConsoleOption types
            for (int i = 0; i < options.Length; i++)
                options[i].Index = i;

            _options = options.ToList();
        }

        public virtual void ListForOption()
        {
            for (;;) // The cooler while (true) :)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ToString());
                string key = Console.ReadLine();

                switch (key)
                {
                    case null:
                        Program.WriteAndClear("Whoops! The entered value returned null. Please only enter actual numbers (1, 5, 27, etc.).");
                        continue;

                    case "/":
                        Program.WriteAndClear("Returned to the start!", ConsoleColor.Green);
                        Program.SelectedOptions.ListForOption();
                        continue;

                    case ".":
                        if (_prevOptionsState == null) 
                            Program.WriteAndClear("No previous state was found, falling back to the beginning...");
                        else
                        {
                            Program.WriteAndClear("Returning to the previous options menu...", ConsoleColor.Green);
                            _prevOptionsState.ListForOption();
                        }

                        continue;
                }

                if (!int.TryParse(key, out int option))
                {
                    Program.WriteAndClear("Whoops! We weren't able to parse your response. Please only enter actual numbers (1, 5, 27, etc.).");
                    continue;
                }

                if (option < 0 || option > Count)
                {
                    Program.WriteAndClear("Whoops! The number entered does not correspond to any of the available options.");
                    continue;
                }

                this.First(x => x.Index == option - 1).Execute();
                break;
            }
        }

        public override string ToString()
        {
            string text = this.Aggregate($" {OptionText}", (current, option) => current + $"\n{option}");

            if (_prevOptionsState != null)
                text += "\n  [.] Return to the previous set of options.";

            return text + "\n  [/] Return to the start.";
        }

        #endregion
        
        #region Painful Interface Code

        public int IndexOf(ConsoleOption item) => _options.IndexOf(item);

        public void Insert(int index, ConsoleOption item) => _options.Insert(index, item);

        public void RemoveAt(int index) => _options.RemoveAt(index);

        public IEnumerator<ConsoleOption> GetEnumerator() => _options.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public object Clone() => MemberwiseClone();

        public void Add(ConsoleOption item) => _options.Add(item);

        public void Clear() => _options.Clear();

        public bool Contains(ConsoleOption item) => _options.Contains(item);

        public void CopyTo(ConsoleOption[] array, int arrayIndex) => _options.CopyTo(array, arrayIndex);

        public bool Remove(ConsoleOption item) => _options.Remove(item);

        public int Count => _options.Count;

        public bool IsReadOnly => false;
        
        #endregion
    }
}
