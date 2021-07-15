using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Consolation.Framework.OptionsSystem
{
    /// <summary>
    ///     A list collection of <see cref="ConsoleOption"/>.
    /// </summary>
    public class ConsoleOptions : ICloneable, IList<ConsoleOption>
    {
        #region ConsoleOptions Code

        public ConsoleOption this[int index]
        {
            get => _options[index];

            set => _options[index] = value;
        }

        /// <summary>
        ///     Prompt text.
        /// </summary>
        public string OptionText { get; }

        /// <summary>
        ///     Whether or not to display a "return" option (returns to default option set).
        /// </summary>
        public bool DisplayReturn { get; set; } = true;

        /// <summary>
        ///     Whether or not to display a "go back" option (returns to the previous option set).
        /// </summary>
        public bool DisplayGoBack { get; set; } = true;

        private readonly List<ConsoleOption> _options;

        private readonly ConsoleOptions? _prevOptionsState;

        /// <summary>
        ///     Constructs a new <see cref="ConsoleOptions"/> instance.
        /// </summary>
        /// <param name="optionText"></param>
        /// <param name="previousOptions"></param>
        /// <param name="options"></param>
        public ConsoleOptions(string optionText, ConsoleOptions? previousOptions = null, params ConsoleOption[] options)
        {
            OptionText = optionText;
            _prevOptionsState = previousOptions;

            // Assign index values to the ConsoleOption types
            for (int i = 0; i < options.Length; i++)
                options[i].Index = i;

            _options = options.ToList();
        }

        /// <summary>
        ///     List all provided options from <see cref="_options"/>, provided in the constructor.
        /// </summary>
        public virtual void ListForOption(ConsoleWindow window)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;

                window.WriteLine(ToString());

                string? key = Console.ReadLine();

                switch (key)
                {
                    case null:
                        window.WriteAndClear("Whoops! The entered value returned null. Please only enter numbers.");
                        continue;

                    case "/" when DisplayReturn:
                        window.WriteAndClear("Returned to the start!", ConsoleColor.Green);
                        window.SelectedOptions = window.DefaultOptions;
                        window.SelectedOptions.ListForOption(window);
                        return;

                    case "." when DisplayGoBack:
                        if (_prevOptionsState == null)
                            window.WriteAndClear("No previous state was found, falling back to the beginning...");
                        else
                        {
                            window.WriteAndClear("Returning to the previous options menu...", ConsoleColor.Green);
                            _prevOptionsState.ListForOption(window);
                        }
                        return;
                }

                if (!int.TryParse(key, out int option))
                {
                    window.WriteAndClear("Whoops! We weren't able to parse your response. Please only enter numbers.");
                    continue;
                }

                if (option < 0 || option > Count)
                {
                    window.WriteAndClear("Whoops! The number entered does not correspond to any available options.");
                    continue;
                }

                this.First(x => x.Index == option - 1).Execute();
                break;
            }
        }

        /// <summary>
        ///     Returns a string listing all options.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string text = this.Aggregate($" {OptionText}", (current, option) => current + $"\n{option}");

            if (DisplayGoBack)
                text += "\n  [.] Return to the previous set of options.";

            if (DisplayReturn)
                return text + "\n  [/] Return to the start.";

            return text;
        }

        #endregion

        #region Painful Interface Code

        public int IndexOf(ConsoleOption item) => _options.IndexOf(item);

        public void Insert(int index, ConsoleOption item)
        {
            _options.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _options.RemoveAt(index);
        }

        public IEnumerator<ConsoleOption> GetEnumerator() => _options.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public object Clone() => MemberwiseClone();

        public void Add(ConsoleOption item)
        {
            _options.Add(item);
        }

        public void Clear()
        {
            _options.Clear();
        }

        public bool Contains(ConsoleOption item) => _options.Contains(item);

        public void CopyTo(ConsoleOption[] array, int arrayIndex)
        {
            _options.CopyTo(array, arrayIndex);
        }

        public bool Remove(ConsoleOption item) => _options.Remove(item);

        public int Count => _options.Count;

        public bool IsReadOnly => false;

        #endregion
    }
}