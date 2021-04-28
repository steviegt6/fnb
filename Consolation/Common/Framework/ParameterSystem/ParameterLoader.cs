using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Consolation.Common.Exceptions;

namespace Consolation.Common.Framework.ParameterSystem
{
    public static class ParameterLoader
    {
        public static List<IParameter>? Parameters { get; private set; }

        /// <summary>
        ///     Finds a parameter matching the given type.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IParameter" /> you want to find.</typeparam>
        /// <returns>An <see cref="IParameter" /> instance if the parameter was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static T? GetParameter<T>() where T : class, IParameter
        {
            return Parameters?.First(x => x.GetType() == typeof(T)) as T;
        }

        /// <summary>
        ///     Finds a loaded parameter matching the specified name or alias.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>An <see cref="IParameter" /> instance if the parameter was found.</returns>
        /// ///
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static IParameter? GetParameter(string parameter)
        {
            return Parameters?.First(x =>
                x.Name.Equals(parameter) || (x.Aliases ?? Array.Empty<string>()).Contains(parameter));
        }

        /// <summary>
        ///     Initialize parameters.
        /// </summary>
        /// <param name="assembly">The assembly to search through.</param>
        public static void Initialize(Assembly assembly)
        {
            Parameters = new List<IParameter>();

            foreach (Type type in assembly.GetTypes().Where(x =>
                !x.IsAbstract && x.GetConstructor(Array.Empty<Type>()) != null && Activator.CreateInstance(x) is IParameter))
                Parameters.Add((Activator.CreateInstance(type) as IParameter)!);

            List<string> namesAndAliases = new();
            foreach (IParameter parameter in Parameters)
            {
                if (!namesAndAliases.Contains(parameter.Name))
                    namesAndAliases.Add(parameter.Name);
                else
                    throw new DuplicateParameterException(parameter.Name);

                if (parameter.Aliases == null)
                    continue;

                foreach (string alias in parameter.Aliases)
                    if (!namesAndAliases.Contains(alias))
                        namesAndAliases.Add(alias);
                    else
                        throw new DuplicateParameterException(alias);
            }
        }
    }
}