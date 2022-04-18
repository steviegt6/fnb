using System;
using System.Collections.Generic;
using System.Reflection;
using Patcher.API.Patching.Loading;

namespace Patcher.Patching.Loading
{
    /// <summary>
    ///     Object to aid in resolving and creating content types.
    /// </summary>
    public class AssemblyContentResolver : IAssemblyContentResolver
    {
        public List<Assembly> Assemblies { get; } = new();

        public List<Type> Types { get; } = new();

        public virtual void AddAssemblies(params Assembly[] assemblies) => Assemblies.AddRange(assemblies);

        public virtual void ResolveTypes()
        {
            foreach (Assembly assembly in Assemblies)
                Types.AddRange(assembly.GetTypes());
        }
        
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