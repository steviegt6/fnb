using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace TML.Patcher.CLI.Common
{
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

        public static string FilePath { get; private set; } = null!;

        public bool ShowIlSpyCmdInstallPrompt { get; set; }

        public string ModsPath { get; set; } = null!;

        public string ExtractPath { get; set; } = null!;

        public string DecompilePath { get; set; } = null!;

        public string ReferencesPath { get; set; } = null!;

        public string RepackPath { get; set; } = null!;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(4)]
        public double Threads { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue((byte) 16)]
        public byte ProgressBarSize { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(10)]
        public int ItemsPerPage { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool ShowRegistryAdditionPrompt { get; set; }

        public static ConfigurationFile? Load(string filePath)
        {
            Patcher window = Program.Patcher;
            FilePath = filePath;

            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(filePath));

            window.WriteLine(1, "Configuration file not found! Generating a new config.json file...");

            ConfigurationFile config = new()
            {
                ShowIlSpyCmdInstallPrompt = true,
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

            string path = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32S => WindowsDefault1,
                PlatformID.Win32Windows => WindowsDefault1,
                PlatformID.Win32NT => WindowsDefault1,
                PlatformID.WinCE => WindowsDefault1,

                PlatformID.Unix => LinuxDefault1,

                PlatformID.MacOSX => MacDefault,

                // LMAO XBOX
                PlatformID.Xbox => UndefinedPath,
                PlatformID.Other => UndefinedPath,
                _ => throw new ArgumentOutOfRangeException(nameof(Environment.OSVersion.Platform), "Invalid platform")
            };

            config.ModsPath = Environment.ExpandEnvironmentVariables(path);

            using (StreamWriter writer = new(filePath))
            using (JsonWriter jWriter = new JsonTextWriter(writer))
            {
                serializer.Serialize(jWriter, config);
            }

            window.WriteLine($"Created a new configuration file in: {filePath}");

            return JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(filePath));
        }

        public static void Save()
        {
            ConfigurationFile config = new()
            {
                ShowIlSpyCmdInstallPrompt = Program.Configuration.ShowIlSpyCmdInstallPrompt,
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
            {
                serializer.Serialize(jWriter, config);
            }

            Program.Configuration = JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(FilePath))!;
        }
    }
}