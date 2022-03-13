using System.IO;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;

namespace TML.Patcher.Tasks
{
    /// <summary>
    ///     Allows you to decompile a .dll using ILSpyCMD.
    /// </summary>
    public class DecompilationTask : ProgressTask
    {
        /// <summary>
        ///     Constructs a new <see cref="DecompilationTask"/> instance.
        /// </summary>
        public DecompilationTask(
            string filePath,
            string decompilePath,
            LanguageVersion version,
            params string[] searchDirectories
            )
        {
            FilePath = filePath;
            DecompilePath = decompilePath;
            Version = version;
            SearchDirectories = searchDirectories;
        }
        
        public string FilePath { get; }
        
        public string DecompilePath { get; }

        public LanguageVersion Version { get; }

        /// <summary>
        ///     The directories to resolve references in.
        /// </summary>
        public string[] SearchDirectories { get; }
        
        public override async Task ExecuteAsync()
        {
            ProgressReporter.Report("Resolving file to decompile.");
            
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("Could not find decompilable file: " + FilePath);

            Directory.CreateDirectory(DecompilePath);

            foreach (string directory in SearchDirectories)
                Directory.CreateDirectory(directory);
            
            ProgressReporter.Report("Preparing to decompile file.");

            PEFile module = new(FilePath);
            UniversalAssemblyResolver resolver = new(FilePath, false, module.Reader.DetectTargetFrameworkId());

            foreach (string directory in SearchDirectories)
                resolver.AddSearchDirectory(directory);

            DecompilerSettings decompilerSettings = new(Version)
            {
                ThrowOnAssemblyResolveErrors = false
            };
            
            ProgressReporter.Report("Initializing decompiler.");

            // Create the decompiler
            WholeProjectDecompiler decompiler = new(decompilerSettings, resolver, resolver, null);
            decompiler.Settings.SetLanguageVersion(Version);

            decompiler.ProgressIndicator = new ActionableProgress<DecompilationProgress>();
            ((ActionableProgress<DecompilationProgress>) decompiler.ProgressIndicator).OnReport += progress =>
            {
                ProgressReporter.Report(new ProgressNotification($"Decompiling file: {progress.Status}"));
            };

            // Decompile the dll and output to the outputDirectory
            decompiler.DecompileProject(module, DecompilePath);

            await Task.CompletedTask;
        }
    }
}