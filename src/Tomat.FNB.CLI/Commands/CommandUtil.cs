using System;
using System.IO;
using System.Threading.Tasks;

using CliFx.Infrastructure;

using Tomat.FNB.Common.IO;
using Tomat.FNB.TMOD;
using Tomat.FNB.TMOD.Converters;

namespace Tomat.FNB.CLI.Commands;

internal static class CommandUtil
{
    public static async ValueTask ExtractArchive(
        IConsole         console,
        TmodFile         tmodFile,
        string           archivePath,
        string?          destinationPath,
        IFileConverter[] converters
    )
    {
        destinationPath ??= Path.GetFileNameWithoutExtension(archivePath);
        await console.Output.WriteLineAsync($"Extracting \"{archivePath}\" to \"{destinationPath}\"...");

        if (Directory.Exists(destinationPath))
        {
            Directory.Delete(destinationPath, true);
        }

#if DEBUG || true
        var watch = System.Diagnostics.Stopwatch.StartNew();
#endif

        tmodFile.ProcessFiles(
            converters,
            (path, bytes) =>
            {
                var dest = Path.Combine(destinationPath, path);
                var dir  = Path.GetDirectoryName(dest);
                if (dir is not null)
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(dest, bytes);
            }
        );

#if DEBUG || true
        watch.Stop();
        await console.Output.WriteLineAsync($"DEBUG: Took {watch.ElapsedMilliseconds}ms");
#endif
    }

    public static TmodFile? ReadTmodFile(string archivePath)
    {
        var fs = File.OpenRead(archivePath);
        var r  = new ByteReader(fs);

        try
        {
            return TmodFile.Read(ref r, new Span<byte>(), new Span<byte>(), ownsStream: true);
        }
        catch
        {
            // Only dispose of them if TmodFile::Read throws so we don't leave
            // them dangling.  Otherwise, TmodFile will assume ownership and we
            // should leave them be.
            r.Dispose();
            fs.Dispose();

            return null;
        }
    }
}