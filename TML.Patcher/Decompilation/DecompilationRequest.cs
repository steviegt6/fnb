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
        public DecompilationRequest(string? file, string decompilePath, string referencesPath, string modName)
        {
            File = file;
            DecompilePath = decompilePath;
            ReferencesPath = referencesPath;
            ModName = modName;
        }

        /// <summary>
        ///     The file path.
        /// </summary>
        public string? File { get; }

        /// <summary>
        ///     The name of the mod.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        ///     The decompilation output path.
        /// </summary>
        public string DecompilePath { get; }

        /// <summary>
        ///     Path for ILSpy decompilation references.
        /// </summary>
        public string ReferencesPath { get; }

        /// <summary>
        ///     Invoked on errors.
        /// </summary>
        public event ErrorMessage? OnError;

        /// <summary>
        ///     Executes the request.
        /// </summary>
        public virtual void ExecuteRequest()
        {
            if (File == null || !FileIO.Exists(File))
            {
                OnError?.Invoke("Unable to locate file to extract.");
                return;
            }

            Directory.CreateDirectory(DecompilePath);
            Directory.CreateDirectory(ReferencesPath);

            Decompile(File, DecompilePath, ReferencesPath, LanguageVersion.CSharp7_3); // TODO: Support CSharp 9 on 1.4
        }

        private static void Decompile(string assemblyFileName, string outputDirectory, string referencePath, LanguageVersion languageVersion)
        {
            PEFile module = new(assemblyFileName);
            UniversalAssemblyResolver resolver = new(assemblyFileName, false, module.Reader.DetectTargetFrameworkId());
            
            // Make the resolver find references in referencePath
            resolver.AddSearchDirectory(referencePath);

            DecompilerSettings decompilerSettings = new(languageVersion) {
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
