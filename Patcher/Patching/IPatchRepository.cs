using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;

namespace Patcher.Patching
{
    /// <summary>
    ///     Represents an object that can handle hook and detour patches.
    /// </summary>
    public interface IPatchRepository
    {
        /// <summary>
        ///     Handles applying and unapplying IL patches.
        /// </summary>
        public readonly struct ILPatch
        {
            /// <summary>
            ///     The method being IL edited.
            /// </summary>
            public readonly MethodInfo BaseMethod;
            
            /// <summary>
            ///     The method performing the IL edit.
            /// </summary>
            public readonly MethodInfo PatchMethod;
            
            /// <summary>
            ///     The resulting <see cref="ILContext.Manipulator"/> delegate.
            /// </summary>
            public readonly Delegate PatchDelegate;

            /// <summary>
            ///     Constructs a new <see cref="IPatchRepository.ILPatch"/> instance, handles the creation of <see cref="PatchDelegate"/>.
            /// </summary>
            /// <param name="baseMethod">The method being IL edited.</param>
            /// <param name="patchMethod">The method performing the IL edit.</param>
            public ILPatch(MethodInfo baseMethod, MethodInfo patchMethod)
            {
                BaseMethod = baseMethod;
                PatchMethod = patchMethod;

                PatchDelegate = Delegate.CreateDelegate(typeof(ILContext.Manipulator), PatchMethod);
            }

            /// <summary>
            ///     Applies the IL edit.
            /// </summary>
            public void Apply() => HookEndpointManager.Modify(BaseMethod, PatchDelegate);

            /// <summary>
            ///     Unapplies the IL edit.
            /// </summary>
            public void Unapply() => HookEndpointManager.Unmodify(BaseMethod, PatchDelegate);
        }

        /// <summary>
        ///     Handles applying and unapplying detour patches.
        /// </summary>
        public readonly struct DetourPatch
        {
            /// <summary>
            ///     The method being detoured.
            /// </summary>
            public readonly MethodInfo BaseMethod;
            
            /// <summary>
            ///     The detouring method.
            /// </summary>
            public readonly MethodInfo PatchMethod;

            /// <summary>
            ///     The resulting detour hook.
            /// </summary>
            public readonly Hook PatchHook;

            /// <summary>
            ///     Constructs a new <see cref="IPatchRepository.DetourPatch"/> instance, handles the creation of <see cref="PatchHook"/>.
            /// </summary>
            /// <param name="baseMethod">The method being detoured.</param>
            /// <param name="patchMethod">The detouring method.</param>
            public DetourPatch(MethodInfo baseMethod, MethodInfo patchMethod)
            {
                BaseMethod = baseMethod;
                PatchMethod = patchMethod;

                PatchHook = new Hook(BaseMethod, PatchMethod);
            }

            /// <summary>
            ///     Applies the detour.
            /// </summary>
            public void Apply() => PatchHook.Apply();

            /// <summary>
            ///     Unapplies the detour.
            /// </summary>
            public void Unapply() => PatchHook.Undo();
        }

        /// <summary>
        ///     All registered IL patches.
        /// </summary>
        List<ILPatch> ILPatches { get; }

        /// <summary>
        ///     All registered detour patches.
        /// </summary>
        List<DetourPatch> DetourPatches { get; }
    }
}