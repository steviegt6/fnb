using System.Diagnostics;
using System.IO;
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

            string commandArgs =
                $"\"{File}\" --referencepath \"{ReferencesPath}\" --outputdir \"{Path.Combine(DecompilePath)}\" --project --languageversion \"CSharp7_3\"";

            ProcessStartInfo ilSpy = new("ilspycmd.exe")
            {
                UseShellExecute = false,
                Arguments = commandArgs
            };

            Process? process = Process.Start(ilSpy);
            process?.WaitForExit();
        }
    }
}