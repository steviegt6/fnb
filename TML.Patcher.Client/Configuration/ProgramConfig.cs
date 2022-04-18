using System;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Spectre.Console;

// no xml comments
#pragma warning disable 1591

namespace TML.Patcher.Client.Configuration
{
    /// <summary>
    ///     JSON-powered simple configuration class.
    /// </summary>
    [DisplayName("programConfig")]
    public sealed class ProgramConfig : JsonConfig<ProgramConfig>
    {
        [JsonProperty("storagePath")] [DefaultValue("undefined")]
        public string StoragePath = "undefined";

        [JsonProperty("steamPath")] [DefaultValue("undefined")]
        public string SteamPath = "undefined";

        [JsonProperty("loaderVersion")] [DefaultValue("legacy")]
        public string LoaderVersion = "legacy";

        [JsonProperty("threads")] [DefaultValue(8D)]
        public double Threads = 8D;

        public string GetStoragePath(string version, out bool valid) =>
            GetStoragePath(GetVersionFromName(version, out valid));

        public string GetStoragePath(ModLoaderVersion version) =>
            System.IO.Path.Combine(StoragePath, version.PathResolver.Invoke(StoragePath));

        public static ModLoaderVersion GetVersionFromName(string version, out bool valid)
        {
            valid = false;
            
            ModLoaderVersion? mlVer = null;

            foreach (ModLoaderVersion ver in ModLoaderVersion.Versions)
            {
                if (!ver.VersionAliases.Contains(version.ToLower()))
                    continue;

                valid = true;
                mlVer = ver;
                break;
            }

            return mlVer ?? ModLoaderVersion.Legacy;
        }

        public static void PrintVersions()
        {
            foreach ((Func<string, string>? pathResolver, bool _, string[]? versionAliases) in ModLoaderVersion.Versions)
            {
                AnsiConsole.MarkupLine($"[gray]Version [white]\"{versionAliases[1]}\"[/] @ [silver]\"{pathResolver.Invoke("")}\"[/][/]");

                for (int i = 0; i < versionAliases.Length; i++)
                {
                    if (i == 0)
                    {
                        AnsiConsole.MarkupLine($"[gray] * \"{versionAliases[i]}\"[/] [lightgoldenrod3](standard)[/]");
                        continue;
                    }
                    
                    AnsiConsole.MarkupLine($"[gray] * \"{versionAliases[i]}\"[/] [red](obsolete)[/]");
                }
                
                AnsiConsole.MarkupLine("\n");
            }
        }
    }
}