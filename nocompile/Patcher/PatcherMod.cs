using System.Collections.Generic;
using System.Linq;
using log4net;
using Patcher.API;
using Patcher.API.IO;
using Patcher.API.Logging;
using Patcher.API.Mod;
using Patcher.IO;
using Patcher.Logging;
using Patcher.Patching;
using Patcher.Patching.Loading;
using Terraria;
using Terraria.ModLoader;

// TODO
namespace Patcher
{
	/// <summary>
	///		Core entrypoint mod used for loading TML.Patcher mods.
	/// </summary>
	public sealed class PatcherMod : Mod, IPatcherMod
	{
		public const string PatcherFolderName = "Patcher";
		public const string ModsFolderName = "Mods";
		public const string ReadMeFileName = "README.txt";

		#region IPatcherMod Impl

		public IFileDirectory PatcherDir { get; } = new FileDirectory(Main.SavePath, PatcherFolderName);
		
		public IFileDirectory ModsDir { get; } = new FileDirectory(Main.SavePath, PatcherFolderName, ModsFolderName);

		/// <summary>
		///		The backing field used for <see cref="WrappedLogger"/>.
		/// </summary>
		private ILogWrapper? BackingWrappedLogger;

		// It's pretty safe to instantiate a new instance every time this is called, not a big deal...
		/// <summary>
		///		A wrapped <see cref="ILog"/> instance. The instance is supplied by <see cref="Mod.Logger"/>.
		/// </summary>
		/// <remarks>
		///		At early stages during mod loading, <see cref="Mod.Logger"/> may be null.
		/// </remarks>
		public ILogWrapper WrappedLogger => Logger is null ?
			new LogWrapper(LogManager.GetLogger(Name)) :
			BackingWrappedLogger ??= new LogWrapper(Logger);

		public List<IPatchRepository.ILPatch> ILPatches { get; } = new();

		public List<IPatchRepository.DetourPatch> DetourPatches { get; } = new();

		public AssemblyContentResolver ContentResolver { get; } = new();
		public List<IPatcher> PatcherMods { get; }

		#endregion

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
			List<IPatch> patches = ContentResolver.GetTypesAsInstances<IPatch>().ToList();
			
			// TODO: Patch sorting.
			// TODO: Later, add ways to apply patches to assemblies (I will do this).
			// TODO: More stuff in the morning.
		}
	}
}