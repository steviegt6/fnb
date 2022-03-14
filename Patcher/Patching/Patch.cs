using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;

namespace Patcher.Patching
{
    /// <summary>
    ///     Represents a detour or IL patch. Use <see cref="ILContext.Manipulator"/> for IL edits, and a delegate for detours.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Patch<T> : Patch where T : Delegate
    {
        /*/// <summary>
        ///     The <see cref="MethodInfo"/> instance to patch.
        /// </summary>
        public abstract MethodInfo ModifiedMethod { get; }*/

        /// <summary>
        ///     The <see cref="MethodInfo"/> being applied as a patch.
        /// </summary>
        public override MethodInfo ModifyingMethod => PatchMethod.Method;

        /// <summary>
        ///     The delegate implementation to use as a patch.
        /// </summary>
        public abstract T PatchMethod { get; }

        /*/// <summary>
        ///     The status of this patch.
        /// </summary>
        public PatchStatus Status { get; set; } = new(false, false);*/

        public override void Apply(IPatchRepository patchRepository)
        {
            if (PatchMethod is ILContext.Manipulator)
            {
                IPatchRepository.ILPatch patch = new(ModifiedMethod, ModifyingMethod);
                patchRepository.ILPatches.Add(patch);
                patch.Apply();
            }
            else
            {
                IPatchRepository.DetourPatch patch = new(ModifiedMethod, ModifyingMethod);
                patchRepository.DetourPatches.Add(patch);
                patch.Apply();

                // Detours are always successful.
                Status = new PatchStatus(true, true);
            }
        }
    }
    
    /// <summary>
    ///     Represents a detour or IL patch.
    /// </summary>
    public abstract class Patch
    {
        /// <summary>
        ///     The <see cref="MethodInfo"/> instance to patch.
        /// </summary>
        public abstract MethodInfo ModifiedMethod { get; }

        /// <summary>
        ///     The <see cref="MethodInfo"/> being applied as a patch.
        /// </summary>
        public abstract MethodInfo ModifyingMethod { get; }

        // /// <summary>
        // ///     The delegate implementation to use as a patch.
        // /// </summary>
        // public abstract T PatchMethod { get; }

        /// <summary>
        ///     The patch this patch is dependent on.
        /// </summary>
        public virtual Type? Dependency => null;

        /// <summary>
        ///     The status of this patch.
        /// </summary>
        public PatchStatus Status { get; set; } = new(false, false);

        public abstract void Apply(IPatchRepository patchRepository);
        /*{
            if (PatchMethod is ILContext.Manipulator)
            {
                IPatchRepository.ILPatch patch = new(ModifiedMethod, ModifyingMethod);
                patchRepository.ILPatches.Add(patch);
                patch.Apply();
            }
            else
            {
                IPatchRepository.DetourPatch patch = new(ModifiedMethod, ModifyingMethod);
                patchRepository.DetourPatches.Add(patch);
                patch.Apply();

                // Detours are always successful.
                Status = new PatchStatus(true, true);
            }
        }*/
    }
}