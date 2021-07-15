using System;
using System.ComponentModel;
using System.IO;
using Consolation;
using Newtonsoft.Json;

namespace TML.Patcher.CLI.Common
{
    /// <summary>
    ///     Configuration files for the program.
    /// </summary>
    public sealed class ConfigurationFile
    {
        public const string UndefinedPath = "undefined";
        public const string WindowsDefault1 = @"%UserProfile%\Documents\My Games\Terraria\ModLoader\Mods";
        public const string WindowsDefault2 = @"%UserProfile%\OneDrive\Documents\My Games\Terraria\ModLoader\Mods";
        public const string MacDefault = @"~/Library/Application support/Terraria/ModLoader/Mods";
        public const string LinuxDefault1 = @"%HOME%/.local/share/Terraria/ModLoader/Mods";
        public const string LinuxDefault2 = @"%XDG_DATA_HOME%/Terraria/ModLoader/Mods";

        internal ConfigurationFile()
        {
        }

        /// <summary>
        ///     Config file path.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public static string FilePath { get; private set; } = null!;

        /// <summary>
        ///     Whether or not to prompt an ILSpyCMD installation.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool ShowILSpyCMDInstallPrompt { get; set; }

        /// <summary>
        ///     Mod directory path.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ModsPath { get; set; } = null!;

        /// <summary>
        ///     Mod extraction path.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ExtractPath { get; set; } = null!;

        /// <summary>
        ///     Mod decompilation path.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string DecompilePath { get; set; } = null!;

        /// <summary>
        ///     ILSpy decompilation references path.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ReferencesPath { get; set; } = null!;

        /// <summary>
        ///     Repack output directory path.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string RepackPath { get; set; } = null!;

        /// <summary>
        ///     The amount of threads to use.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(4D)]
        public double Threads { get; set; }

        /// <summary>
        ///     Rendered <see cref="ProgressBar"/> size.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue((byte) 16)]
        public byte ProgressBarSize { get; set; }

        /// <summary>
        ///     Amount of items per page for browsing.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(10)]
        public int ItemsPerPage { get; set; }

        /// <summary>
        ///     Whether or not to prompt a registry insertion prompt.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool ShowRegistryAdditionPrompt { get; set; }

        /// <summary>
        ///     Loads a config file at the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ConfigurationFile? Load(string path)
        {
            Patcher window = Program.Patcher;
            FilePath = path;

            if (File.Exists(path))
                return JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(path));

            window.WriteLine(" Configuration file not found! Generating a new config.json file...");

            ConfigurationFile config = new()
            {
                ShowILSpyCMDInstallPrompt = true,
                ExtractPath = Path.Combine(Program.ExePath, "Extracted"),
                DecompilePath = Path.Combine(Program.ExePath, "Decompiled"),
                ReferencesPath = Path.Combine(Program.ExePath, "References"),
                RepackPath = Path.Combine(Program.ExePath, "Repacked"),
                Threads = 4,
                ProgressBarSize = 16,
                ItemsPerPage = 10,
                ShowRegistryAdditionPrompt = true
            };

            JsonSerializer serializer = new()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string platformPath = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32S => WindowsDefault1,
                PlatformID.Win32Windows => WindowsDefault1,
                PlatformID.Win32NT => WindowsDefault1,
                PlatformID.WinCE => WindowsDefault1,

                PlatformID.Unix => LinuxDefault1,

                PlatformID.MacOSX => MacDefault,

                PlatformID.Xbox => UndefinedPath,
                PlatformID.Other => UndefinedPath,

                _ => throw new ArgumentOutOfRangeException(nameof(Environment.OSVersion.Platform), "Invalid platform")
            };

            config.ModsPath = Environment.ExpandEnvironmentVariables(platformPath);

            using (StreamWriter writer = new(path))
            using (JsonWriter jWriter = new JsonTextWriter(writer))
                serializer.Serialize(jWriter, config);

            window.WriteLine($"Created a new configuration file in: {path}");

            return JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(path));
        }

        /// <summary>
        ///     Saves the current values to the config.
        /// </summary>
        public static void Save()
        {
            ConfigurationFile config = new()
            {
                ShowILSpyCMDInstallPrompt = Program.Configuration.ShowILSpyCMDInstallPrompt,
                ModsPath = Program.Configuration.ModsPath,
                ExtractPath = Program.Configuration.ExtractPath,
                DecompilePath = Program.Configuration.DecompilePath,
                ReferencesPath = Program.Configuration.ReferencesPath,
                RepackPath = Program.Configuration.RepackPath,
                // TODO: give extract, decompile, & references default values that don't save to the config for portability
                Threads = Program.Configuration.Threads,
                ProgressBarSize = Program.Configuration.ProgressBarSize,
                ItemsPerPage = Program.Configuration.ItemsPerPage,
                ShowRegistryAdditionPrompt = Program.Configuration.ShowRegistryAdditionPrompt
            };

            JsonSerializer serializer = new()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            using (StreamWriter writer = new(FilePath))
            using (JsonWriter jWriter = new JsonTextWriter(writer))
                serializer.Serialize(jWriter, config);

            Program.Configuration = JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(FilePath))!;
        }
    }
}