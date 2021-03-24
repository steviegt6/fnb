using System;

namespace Consolation.Common.Exceptions
{
    public class DuplicateParameterException : Exception
    {
        public string DuplicateNameOrAlias { get; }

        public DuplicateParameterException(string nameOrAlias) => DuplicateNameOrAlias = nameOrAlias;

        public override string Message => $"Error while attempting to register a parameter with the name or alias \"{DuplicateNameOrAlias}\" as a registered parameter already has the name or alias specified!";
    }
}
