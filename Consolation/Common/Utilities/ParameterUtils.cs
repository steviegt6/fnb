using System.Collections.Generic;
using System.Linq;
using Consolation.Common.Framework.ParameterSystem;

namespace Consolation.Common.Utilities
{
    /// <summary>
    ///     A utility class containing numerous methods designed to easily manage and keep track of launch parameters.
    /// </summary>
    public static class ParameterUtils
    {
        /// <summary>
        ///     Attempts to get the specified parameter, along with its value.
        /// </summary>
        /// <param name="args">The array of parameter arguments you want to check.</param>
        /// <param name="parameter">The parameter you want to get the value of.</param>
        /// <param name="parameterValue">The vale of the parameter, returns <see cref="string.Empty" /> if there is no value.</param>
        /// <returns><c>true</c> if the specified parameter is found, else <c>false</c>.</returns>
        public static bool TryGetParameterValue(this string[] args, string parameter, out string parameterValue)
        {
            int index = args.FindParameter(parameter);
            string value = args[index + 1];
            parameterValue = value.StartsWith('-')
                ? string.Empty
                : value;

            return index > -1 && index < args.Length - 1;
        }

        /// <summary>
        ///     Attempts to get the specified parameter.
        /// </summary>
        /// <param name="args">The array of parameter arguments you want to check.</param>
        /// <param name="parameter">The parameter you want to get the value of.</param>
        /// <returns><c>true</c> if the specified parameter is found, else <c>false</c>.</returns>
        public static bool ParameterExists(this string[] args, string parameter) =>
            args.TryGetParameterValue(parameter, out _);

        /// <summary>
        ///     Finds the <c>index</c> of the specified parameter by converting <paramref name="args" /> to a
        ///     <see cref="List{T}" /> with <see cref="Enumerable.ToList{T}" /> and then returning
        ///     <see cref="List{T}.IndexOf(T)" />
        /// </summary>
        /// <param name="args">The array of parameter arguments you want to check.</param>
        /// <param name="parameter">The parameter you want to get the value of.</param>
        /// <returns>The <c>index</c> of the specified parameter. <c>-1</c> if the specified parameter is not found.</returns>
        public static int FindParameter(this string[] args, string parameter) => args.ToList().IndexOf(parameter);

        /// <summary>
        ///     Parses the specified parameter if it exists.
        /// </summary>
        /// <param name="args">The array of parameter arguments you want to check.</param>
        /// <param name="parameter">The parameter you want to parse.</param>
        public static void ParseParameter(this string[] args, string parameter)
        {
            try
            {
                IParameter param = ParameterLoader.GetParameter(parameter);

                if (param.ExpectsValue)
                {
                    if (args.TryGetParameterValue(param.Name, out string value))
                        param.Parse(value);
                }
                else
                {
                    param.Parse("");
                }
            }
            catch
            {
                /* ignored */
            }
        }
    }
}