using System.Diagnostics;
using System.IO;
using FileIO = System.IO.File;

namespace TML.Patcher.Backend.Decompilation
{
    public sealed class DecompilationRequest
    {
        public delegate void ErrorMessage(string message);

        public DecompilationRequest(string? file, string decompilePath, string referencesPath, string modName)
        {
            File = file;
            DecompilePath = decompilePath;
            ReferencesPath = referencesPath;
            ModName = modName;
        }

        public string? File { get; }

        public string ModName { get; }

        public string DecompilePath { get; }

        public string ReferencesPath { get; }

        public event ErrorMessage? OnError;

        public void ExecuteRequest()
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