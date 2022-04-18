using System;
using System.Reflection;
using MonoMod.Cil;

namespace Patcher.Patching
{
    /// <summary>
    ///     Represents a detour or IL patch. Use <see cref="ILContext.Manipulator"/> for IL edits, and a delegate for detours.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Patch<T> : IPatch where T : Delegate
    {
        public abstract MethodInfo ModifiedMethod { get; }

        public MethodInfo ModifyingMethod => PatchMethod.Method;

        object IPatch.PatchMethod => PatchMethod;
        
        public abstract T PatchMethod { get; }

        public virtual Type? Dependency => null;

        public PatchStatus Status { get; set; } = new(false, false);

        public void Apply(IPatchRepository patchRepository)
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
}