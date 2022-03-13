using System;
using System.Reflection;
using MonoMod.Cil;

namespace Patcher.Patching
{
    /// <summary>
    ///     Represents a detour or IL patch. Use <see cref="ILContext.Manipulator"/> for IL edits, and a delegate for detours.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Patch<T> where T : Delegate
    {
        /// <summary>
        ///     The <see cref="MethodInfo"/> instance to patch.
        /// </summary>
        public abstract MethodInfo ModifiedMethod { get; }

        /// <summary>
        ///     The <see cref="MethodInfo"/> being applied as a patch.
        /// </summary>
        public virtual MethodInfo ModifyingMethod => PatchMethod.Method;

        /// <summary>
        ///     The delegate implementation to use as a patch.
        /// </summary>
        public abstract T PatchMethod { get; }

        public virtual void Apply(IPatchRepository patchRepository)
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
            }
        }
    }

    public class Foo : Patch<ILContext.Manipulator>
    {
        public override MethodInfo ModifiedMethod { get; }
        
        public override ILContext.Manipulator PatchMethod { get; }
    }

    public class Bar : Patch<Bar.Baz>
    {
        public delegate void Baz();

        public override MethodInfo ModifiedMethod { get; }
        
        public override Baz PatchMethod { get; }
    }
}