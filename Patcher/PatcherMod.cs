using System.Collections.Generic;
using System.Linq;
using log4net;
using Patcher.IO;
using Patcher.Loading;
using Patcher.Logging;
using Patcher.Patching;
using Terraria;
using Terraria.ModLoader;

namespace Patcher
{
	/// <summary>
	///		Core entrypoint mod used for loading TML.Patcher mods.
	/// </summary>
	public sealed class PatcherMod : Mod, IPatchRepository
	{
		public const string PatcherFolderName = "Patcher";
		public const string ModsFolderName = "Mods";
		public const string ReadMeFileName = "README.txt";
		
		public readonly FileDirectory PatcherDir = new(Main.SavePath, PatcherFolderName);
		public readonly FileDirectory ModsDir = new(Main.SavePath, PatcherFolderName, ModsFolderName);

		/// <summary>
		///		The backing field used for <see cref="WrappedLogger"/>.
		/// </summary>
		private LogWrapper? BackingWrappedLogger;

		// At early stages during mod loading, Logger may be null.
		// It's pretty safe to instantiate a new instance every time this is called, not a big deal...
		/// <summary>
		///		A wrapped <see cref="ILog"/> instance. The instance is supplied by <see cref="Mod.Logger"/>.
		/// </summary>
		public LogWrapper WrappedLogger => Logger is null ?
			new LogWrapper(LogManager.GetLogger(Name)) :
			BackingWrappedLogger ??= new LogWrapper(Logger);

		public List<IPatchRepository.ILPatch> ILPatches { get; } = new();

		public List<IPatchRepository.DetourPatch> DetourPatches { get; } = new();

		private readonly AssemblyContentResolver ContentResolver = new();

		public override void Load()
		{
			base.Load();
			
			ContentResolver.AddAssemblies(/* TODO: Resolve assemblies here. */);
			ContentResolver.ResolveTypes();
			
			LoadPatches();
		}

		public override void Unload()
		{
			base.Unload();
			
			foreach (IPatchRepository.ILPatch patch in ILPatches)
				patch.Unapply();
			
			foreach (IPatchRepository.DetourPatch patch in DetourPatches)
				patch.Unapply();
		}

		private void LoadPatches()
		{
			List<Patch> patches = ContentResolver.GetTypesAsInstances<Patch>().ToList();
			
			// TODO: Patch sorting.
			// TODO: Later, add ways to apply patches to assemblies (I will do this).
			// TODO: More stuff in the morning.
		}
	}
}