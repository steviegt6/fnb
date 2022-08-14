using System;
using System.Collections.Generic;
using System.Reflection;

namespace Patcher.API.Patching.Loading
{
    /// <summary>
    ///     Object to aid in resolving and creating content types.
    /// </summary>
    public interface IAssemblyContentResolver
    {
        /// <summary>
        ///     A collection of assemblies used for content resolution.
        /// </summary>
        List<Assembly> Assemblies { get; }

        /// <summary>
        ///     A collection of types from each assembly.
        /// </summary>
        List<Type> Types { get; }

        /// <summary>
        ///     Adds the specified assemblies to the list of internal assemblies to resolve types from.
        /// </summary>
        void AddAssemblies(params Assembly[] assemblies);

        /// <summary>
        ///     Resolves all types from the added assemblies.
        /// </summary>
        void ResolveTypes();
        
        /// <summary>
        ///     Resolves an instance for each type.
        /// </summary>
        /// <param name="isInterface">Whether the type is an instance.</param>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>A collection of instances.</returns>
        IEnumerable<T> GetTypesAsInstances<T>(bool isInterface = false);
    }
}