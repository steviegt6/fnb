using System.Reflection;
using Consolation.Common;
using Consolation.Common.Framework.OptionsSystem;
using Consolation.Common.Framework.ParameterSystem;
using Consolation.Common.Utilities;

namespace Consolation
{
    /// <summary>
    ///     Core <c>Consolation</c> class.
    /// </summary>
    public static class Consolation
    {
        public static ConsoleWindow Window { get; set; } = null!;

        public static ConsoleOptions SelectedOptionSet { get; set; } = null!;

        /// <summary>
        ///     Initializes <c>Consolation</c> systems.
        /// </summary>
        public static void Initialize()
        {
            ParameterLoader.Initialize(Assembly.GetCallingAssembly());
        }

        public static void ParseParameters(string[] args)
        {
            foreach (string paramCandidate in args)
                args.ParseParameter(paramCandidate);
        }

        /// <summary>
        ///     Returns <see cref="Window" /> but cast to <typeparamref name="TWindow"></typeparamref>.
        /// </summary>
        public static TWindow GetWindow<TWindow>() where TWindow : ConsoleWindow => (TWindow) Window;
    }
}