using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using CliFx.Infrastructure;

using Microsoft.Win32;

using Tomat.FNB.TMOD;
using Tomat.FNB.TMOD.Converters.Extractors;
using Tomat.FNB.TMOD.Utilities;

namespace Tomat.FNB.Commands;

/// <summary>
///     Command utilities.
/// </summary>
internal static class CommandUtil
{
    /// <summary>
    ///     The tModLoader Steam app ID.
    /// </summary>
    public const int TMODLOADER_APPID = 1281930;

    /// <summary>
    ///     Known possible tModLoader "mod loader" directory names.
    /// </summary>
    private static readonly string[] mod_loader_dir_candidates = { "tModLoader", "tModLoader-1.4.3", "tModLoader-preview", "tModLoader-dev", "ModLoader" };

    /// <summary>
    ///     Helper method to extract a .tmod archive at a given path to a given
    ///     destination path.
    /// </summary>
    /// <param name="console">
    ///     The console to write to.
    /// </param>
    /// <param name="archivePath">
    ///     The path to the .tmod archive.
    /// </param>
    /// <param name="destinationPath">
    ///     The destination path to extract to. If <see langword="null"/>, the
    ///     destination path will be the name of the archive without the extension.
    /// </param>
    public static async ValueTask ExtractArchive(IConsole console, string archivePath, string? destinationPath)
    {
        destinationPath ??= Path.GetFileNameWithoutExtension(archivePath);
        await console.Output.WriteLineAsync($"Extracting \"{archivePath}\" to \"{destinationPath}\"...");

        if (Directory.Exists(destinationPath))
            Directory.Delete(destinationPath, true);

#if DEBUG || true
        var watch = System.Diagnostics.Stopwatch.StartNew();
#endif

        IReadOnlyTmodFile tmodFile;
        try
        {
            await using var fs = File.OpenRead(archivePath);
            {
                var serializableTmodFile = SerializableTmodFile.FromStream(fs);
                tmodFile = serializableTmodFile.Convert(
                    [RawimgExtractor.GetRawimgExtractor(), new InfoExtractor()],
                    action:
                    (addFile, path, data) =>
                    {
                        addFile(path, data);

                        var dest = Path.Combine(destinationPath, path);

                        var dir = Path.GetDirectoryName(dest);
                        if (dir is not null)
                        {
                            Directory.CreateDirectory(dir);
                        }

                        File.WriteAllBytes(dest, data);
                    }
                );
            }
        }
        catch (Exception e)
        {
            await console.Error.WriteLineAsync($"Failed to read \"{archivePath}\": {e}");
            return;
        }

#if DEBUG || true
        watch.Stop();
        await console.Output.WriteLineAsync($"DEBUG: Took {watch.ElapsedMilliseconds}ms");
#endif
    }

    /// <summary>
    ///     Attempts to find the local tModLoader archives for the given
    ///     directory.
    /// </summary>
    /// <param name="localMods">
    ///     The local tModLoader archives, if found.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the local tModLoader archives were found;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetLocalTmodArchives([NotNullWhen(returnValue: true)] out Dictionary<string, Dictionary<string, string>>? localMods)
    {
        if (!TryGetTerrariaStoragePath(out var storageDir))
        {
            localMods = null;
            return false;
        }

        localMods = new Dictionary<string, Dictionary<string, string>>();

        foreach (var candidate in mod_loader_dir_candidates)
        {
            var dir = Path.Combine(storageDir, candidate);
            if (!Directory.Exists(dir))
                continue;

            var modsDir = Path.Combine(dir, "Mods");
            if (!Directory.Exists(modsDir))
                continue;

            var modsMap = new Dictionary<string, string>();

            foreach (var mod in Directory.EnumerateFiles(modsDir, "*.tmod", SearchOption.TopDirectoryOnly))
            {
                var modName = Path.GetFileName(mod);
                modsMap.Add(modName, mod);
            }

            localMods.Add(candidate, modsMap);
        }

        return true;
    }

    /// <summary>
    ///     Attempts to find the path to the Terraria storage directory, which
    ///     is where the game saves its transient files.
    /// </summary>
    /// <param name="storageDir">
    ///     The Terraria storage directory, if found.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the storage directory was found;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetTerrariaStoragePath([NotNullWhen(returnValue: true)] out string? storageDir)
    {
        storageDir = null;

        if (OperatingSystem.IsWindows())
        {
            storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games");
        }
        else if (OperatingSystem.IsLinux())
        {
            if (Environment.GetEnvironmentVariable("XDG_DATA_HOME") is not { } xdgDataHome)
                storageDir = Environment.GetEnvironmentVariable("HOME") is not { } home ? "." : Path.Combine(home, ".local", "share");
            else
                storageDir = xdgDataHome;
        }
        else if (OperatingSystem.IsMacOS())
        {
            storageDir = Environment.GetEnvironmentVariable("HOME") is not { } home ? "." : Path.Combine(home, "Library", "Application Support");
        }

        if (storageDir is null)
            return false;

        storageDir = Path.Combine(storageDir, "Terraria");
        return true;
    }

    /// <summary>
    ///     Resolves the tModLoader Workshop entries for the given directory.
    /// </summary>
    /// <param name="workshopDir">
    ///     The tModLoader Workshop directory.
    /// </param>
    /// <returns>
    ///     A map of tModLoader Workshop entries.
    /// </returns>
    public static Dictionary<string, TmodWorkshopRecord> ResolveTmodWorkshopEntries(string workshopDir)
    {
        var map = new Dictionary<string, TmodWorkshopRecord>();

        foreach (var dir in Directory.EnumerateDirectories(workshopDir))
        {
            var dirName = Path.GetFileName(dir);

            if (!long.TryParse(dirName, out var itemId))
                continue;

            var items = new List<TmodWorkshopItem>();

            var rootTmods = Directory.EnumerateFiles(dir, "*.tmod", SearchOption.TopDirectoryOnly);

            foreach (var rootTmod in rootTmods)
            {
                var tmodName = Path.GetFileName(rootTmod);
                items.Add(new TmodWorkshopItem(null, tmodName, rootTmod));
            }

            foreach (var version in Directory.EnumerateDirectories(dir))
            {
                var versionName = Path.GetFileName(version);

                var tmods = Directory.EnumerateFiles(version, "*.tmod", SearchOption.TopDirectoryOnly);

                foreach (var tmod in tmods)
                {
                    var tmodName = Path.GetFileName(tmod);
                    items.Add(new TmodWorkshopItem(versionName, tmodName, tmod));
                }
            }

            map.Add(dirName, new TmodWorkshopRecord(itemId, items));
        }

        return map;
    }

    /// <summary>
    ///     Attempts to find the tModLoader workshop directory for the given
    ///     Steam app ID.
    /// </summary>
    /// <param name="appId">The Steam app ID.</param>
    /// <param name="workshopDir">
    ///     The workshop directory, if found.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the workshop directory was found;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetWorkshopDirectory(int appId, [NotNullWhen(returnValue: true)] out string? workshopDir)
    {
        if (!TryGetSteamDirectory(out var steamDir))
        {
            workshopDir = null;
            return false;
        }

        workshopDir = Path.Combine(steamDir, "steamapps", "workshop", "content", appId.ToString());

        if (Directory.Exists(workshopDir))
            return true;

        workshopDir = null;
        return false;
    }

    /// <summary>
    ///     Attempts to automatically find the Steam directory on the system.
    /// </summary>
    /// <param name="steamDir">
    ///     The Steam directory, if found.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the Steam directory was found; otherwise,
    ///     <see langword="false"/>.
    /// </returns>
    public static bool TryGetSteamDirectory([NotNullWhen(returnValue: true)] out string? steamDir)
    {
        foreach (var dir in getSteamDirectories())
        {
            if (dir is null)
                continue;

            if (!Directory.Exists(dir))
                continue;

            steamDir = dir;
            return true;
        }

        steamDir = null;
        return false;

        IEnumerable<string?> getSteamDirectories()
        {
            // If HOME exists, we can use known Linux and MacOS paths.
            // If not, we'll use Windows paths instead.

            if (Environment.GetEnvironmentVariable("HOME") is { } home)
            {
                yield return Path.Combine(home, ".steam",  "steam");
                yield return Path.Combine(home, ".local",  "share",               "Steam");
                yield return Path.Combine(home, ".var",    "app",                 "com.valvesoftware.Steam", "data", "Steam");
                yield return Path.Combine(home, "Library", "Application Support", "Steam");
            }

            if (OperatingSystem.IsWindows())
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) is string steamPath)
                    yield return steamPath;

                yield return @"C:\Program Files (x86)\Steam";
                yield return @"C:\Program Files\Steam";
            }
        }
    }
}