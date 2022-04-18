using System.Collections.Generic;
using Patcher.API.IO;
using Patcher.API.Logging;
using Patcher.API.Mod;
using Patcher.API.Patching.Loading;
using Patcher.Patching;
using Patcher.Patching.Loading;

namespace Patcher.API
{
    public interface IPatcherMod : IPatchRepository
    {
        IFileDirectory PatcherDir { get; }

        IFileDirectory ModsDir { get; }

        ILogWrapper WrappedLogger { get; }

        AssemblyContentResolver ContentResolver { get; }

        List<IPatcher> PatcherMods { get; }
    }
}