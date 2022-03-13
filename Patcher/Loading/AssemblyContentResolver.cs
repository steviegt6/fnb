using System;
using System.Collections.Generic;
using System.Reflection;

namespace Patcher.Loading
{
    /// <summary>
    ///     Object to aid in resolving and creating content types.
    /// </summary>
    public class AssemblyContentResolver
    {
        /// <summary>
        ///     A collection of assemblies used for content resolution.
        /// </summary>
        protected readonly List<Assembly> Assemblies = new();

        /// <summary>
        ///     A collection of types from each assembly.
        /// </summary>
        protected readonly List<Type> Types = new();

        /// <summary>
        ///     Adds the specified assemblies to the list of internal assemblies to resolve types from.
        /// </summary>
        public virtual void AddAssemblies(params Assembly[] assemblies) => Assemblies.AddRange(assemblies);

        /// <summary>
        ///     Resolves all types from the added assemblies.
        /// </summary>
        public virtual void ResolveTypes()
        {
            foreach (Assembly assembly in Assemblies)
                Types.AddRange(assembly.GetTypes());
        }

        /// <summary>
        ///     Resolves an instance for each type.
        /// </summary>
        /// <param name="isInterface">Whether the type is an instance.</param>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>A collection of instances.</returns>
        public virtual IEnumerable<T> GetTypesAsInstances<T>(bool isInterface = false)
        {
            foreach (Type type in Types)
            {
                if (type.IsAbstract || type.GetConstructor(Array.Empty<Type>()) is null)
                    continue;

                switch (isInterface)
                {
                    case true when !typeof(T).IsAssignableFrom(type):
                    case false when !type.IsSubclassOf(typeof(T)):
                        continue;
                    
                    default:
                        yield return (T) Activator.CreateInstance(type)!;
                        break;
                }
            }
        }
    }
}