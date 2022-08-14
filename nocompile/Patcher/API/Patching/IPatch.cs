using System;
using System.Reflection;

namespace Patcher.Patching
{
    /// <summary>
    ///     Represents a detour or IL patch.
    /// </summary>
    public interface IPatch
    {
        /// <summary>
        ///     The <see cref="MethodInfo"/> instance to patch.
        /// </summary>
        MethodInfo ModifiedMethod { get; }

        /// <summary>
        ///     The <see cref="MethodInfo"/> being applied as a patch.
        /// </summary>
        MethodInfo ModifyingMethod { get; }

        // /// <summary>
        // ///     The delegate implementation to use as a patch.
        // /// </summary>
        object PatchMethod { get; }

        /// <summary>
        ///     The patch this patch is dependent on.
        /// </summary>
        Type? Dependency { get; }

        /// <summary>
        ///     The status of this patch.
        /// </summary>
        PatchStatus Status { get; set; }

        void Apply(IPatchRepository patchRepository);
    }
}