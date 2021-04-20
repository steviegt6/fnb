using System;
using System.Reflection;
using Consolation.Common;
using Consolation.Common.Framework.OptionsSystem;
using Consolation.Common.Framework.ParameterSystem;
using Consolation.Common.Utilities;

namespace Consolation
{
    /// <summary>
    /// Core <c>Consolation</c> class.
    /// </summary>
    public static class ConsoleAPI
    {
        public static Version ConsolationVersion => new(1, 0, 0, 0);

        /// <summary>
        /// Initializes <c>Consolation</c> systems.
        /// </summary>
        public static void Initialize() => ParameterLoader.Initialize(Assembly.GetCallingAssembly());

        public static ConsoleWindow Window { get; set; }

        public static ConsoleOptions SelectedOptionSet { get; set; }

        public static void ParseParameters(string[] args)
        {
            foreach (string paramCandidate in args)
                args.ParseParameter(paramCandidate);
        }
    }
}
