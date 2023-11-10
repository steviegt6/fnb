using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Win32;

namespace Tomat.FNB.Commands;

internal static class CommandUtil {
    public const int TMODLOADER_APPID = 1281930;

    public static Dictionary<string, TmodWorkshopRecord> ResolveTmodWorkshopEntries(string workshopDir) {
        var map = new Dictionary<string, TmodWorkshopRecord>();

        foreach (var dir in Directory.EnumerateDirectories(workshopDir)) {
            var dirName = Path.GetFileName(dir);

            if (!long.TryParse(dirName, out var itemId))
                continue;

            var items = new List<TmodWorkshopItem>();

            var rootTmods = Directory.EnumerateFiles(dir, "*.tmod", SearchOption.TopDirectoryOnly);

            foreach (var rootTmod in rootTmods) {
                var tmodName = Path.GetFileName(rootTmod);
                items.Add(new TmodWorkshopItem(null, tmodName));
            }

            foreach (var version in Directory.EnumerateDirectories(dir)) {
                var versionName = Path.GetFileName(version);

                var tmods = Directory.EnumerateFiles(version, "*.tmod", SearchOption.TopDirectoryOnly);

                foreach (var tmod in tmods) {
                    var tmodName = Path.GetFileName(tmod);
                    items.Add(new TmodWorkshopItem(versionName, tmodName));
                }
            }

            map.Add(dirName, new TmodWorkshopRecord(itemId, items));
        }

        return map;
    }

    public static bool TryGetWorkshopDirectory(int appId, [NotNullWhen(returnValue: true)] out string? workshopDir) {
        if (!TryGetSteamDirectory(out var steamDir)) {
            workshopDir = null;
            return false;
        }

        workshopDir = Path.Combine(steamDir, "steamapps", "workshop", "content", appId.ToString());

        if (Directory.Exists(workshopDir))
            return true;

        workshopDir = null;
        return false;
    }

    public static bool TryGetSteamDirectory([NotNullWhen(returnValue: true)] out string? steamDir) {
        foreach (var dir in getSteamDirectories()) {
            if (dir is null)
                continue;

            if (!Directory.Exists(dir))
                continue;

            steamDir = dir;
            return true;
        }

        steamDir = null;
        return false;

        IEnumerable<string?> getSteamDirectories() {
            if (Environment.GetEnvironmentVariable("HOME") is { } home) {
                yield return Path.Combine(home, ".steam", "steam");
                yield return Path.Combine(home, ".local", "share", "Steam");
                yield return Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", "data", "Steam");
                yield return Path.Combine(home, "Library", "Application Support", "Steam");
            }

            if (OperatingSystem.IsWindows()) {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) is string steamPath)
                    yield return steamPath;

                yield return @"C:\Program Files (x86)\Steam";
                yield return @"C:\Program Files\Steam";
            }
        }
    }
}
