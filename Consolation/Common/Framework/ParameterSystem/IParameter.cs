namespace Consolation.Common.Framework.ParameterSystem
{
    public interface IParameter
    {
        /// <summary>
        /// The parameter's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A <c>nullable</c> array of parameter aliases.
        /// </summary>
        string[]? Aliases { get; }

        /// <summary>
        /// Whether or not this parameter expects an accompanying value.
        /// </summary>
        bool ExpectsValue { get; }

        /// <summary>
        /// Allows you to execute code if this parameter was flagged.
        /// </summary>
        /// <param name="value">The parameter's value if there was on specified and <see cref="ExpectsValue"/> is <c>true</c>, otherwise <see cref="string.Empty"/>.</param>
        void Parse(string value);
    }
}
