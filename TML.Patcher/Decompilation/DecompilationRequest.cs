using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;
using FileIO = System.IO.File;

namespace TML.Patcher.Decompilation
{
    /// <summary>
    ///     Allows you to decompile a .dll using ILSpyCMD.
    /// </summary>
    public class DecompilationRequest
    {
        /// <summary>
        ///     Invoked on errors. Allows you to output to the console.
        /// </summary>
        /// <param name="message"></param>
        public delegate void ErrorMessage(string message);

        /// <summary>
        ///     Constructs a new <see cref="DecompilationRequest"/> instance.
        /// </summary>
        public DecompilationRequest(
            string file,
            string decompilePath,
            string modName,
            LanguageVersion version,
            params string[] searchDirectories)
        {
            File = file;
            DecompilePath = decompilePath;
            ModName = modName;
            Version = version;
            SearchDirectories = searchDirectories;
        }

        /// <summary>
        ///     The file path.
        /// </summary>
        public string File { get; }

        /// <summary>
        ///     The name of the mod.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        ///     The decompilation output path.
        /// </summary>
        public string DecompilePath { get; }

        /// <summary>
        ///     The C# language version.
        /// </summary>
        public LanguageVersion Version { get; }

        /// <summary>
        ///     The directories to resolve references in.
        /// </summary>
        public string[] SearchDirectories { get; }

        /// <summary>
        ///     Executes the request.
        /// </summary>
        public virtual void ExecuteRequest()
        {
            if (!FileIO.Exists(File))
                throw new Exception("Could not find extractable file: " + File);

            Directory.CreateDirectory(DecompilePath);

            foreach (string directory in SearchDirectories)
                Directory.CreateDirectory(directory);

            Decompile(File, DecompilePath, SearchDirectories, Version);
        }

        private static void Decompile(
            string assemblyFileName,
            string outputDirectory,
            IEnumerable<string> searchDirectories,
            LanguageVersion languageVersion
        )
        {
            PEFile module = new(assemblyFileName);
            UniversalAssemblyResolver resolver = new(assemblyFileName, false, module.Reader.DetectTargetFrameworkId());

            foreach (string directory in searchDirectories)
                resolver.AddSearchDirectory(directory);

            DecompilerSettings decompilerSettings = new(languageVersion)
            {
                ThrowOnAssemblyResolveErrors = false
            };

            // Create the decompiler
            WholeProjectDecompiler decompiler = new(decompilerSettings, resolver, resolver, null);
            decompiler.Settings.SetLanguageVersion(languageVersion);

            // Decompile the dll and output to the outputDirectory
            decompiler.DecompileProject(module, outputDirectory);
        }
    }
}