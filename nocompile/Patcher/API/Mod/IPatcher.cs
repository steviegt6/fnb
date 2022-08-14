using System.Collections.Generic;
using System.Reflection;
using Patcher.API.Patching.Loading;

namespace Patcher.API.Mod
{
    /// <summary>
    ///     The core data in a patcher mod.
    /// </summary>
    public interface IPatcher
    {
        /// <summary>
        ///     This mod's assembly.
        /// </summary>
        Assembly Code { get; }
        
        /// <summary>
        ///     All loadable instances in this assembly.
        /// </summary>
        List<IPatcherLoadable> Loadables { get; }
        
        /// <summary>
        ///     Runs when this patcher mod is loaded.
        /// </summary>
        void Load();

        
        /// <summary>
        ///     Runs when this patcher mod is unloaded.
        /// </summary>
        void Unload();
    }
}