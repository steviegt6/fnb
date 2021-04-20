using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace TML.Patcher.Frontend.Common
{
    public sealed class ConfigurationFile
    {
        public const string UndefinedPath = "undefined";
        public const string WindowsDefault = @"%UserProfile%\Documents\My Games\Terraria\ModLoader\Mods";
        public const string MacDefault = @"~/Library/Application support/Terraria/ModLoader/Mods";
        public const string LinuxDefault1 = @"%HOME%/.local/share/Terraria/ModLoader/Mods";
        public const string LinuxDefault2 = @"%XDG_DATA_HOME%/Terraria/ModLoader/Mods";

        public static string FilePath { get; private set; }
        
        public bool ShowIlSpyCmdInstallPrompt { get; set; }

        public string ModsPath { get; set; }

        public string ExtractPath { get; set; }

        public string DecompilePath { get; set; }

        public string ReferencesPath { get; set; }
        
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(4)]
        public double Threads { get; set; }

        internal ConfigurationFile() { }

        public static ConfigurationFile Load(string filePath)
        {
            FilePath = filePath;

            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(filePath));

            Console.WriteLine(" Configuration file not found! Generating a new config.json file...");

            ConfigurationFile config = new()
            {
                ShowIlSpyCmdInstallPrompt = true,
                ExtractPath = Path.Combine(Program.EXEPath, "Extracted"),
                DecompilePath = Path.Combine(Program.EXEPath, "Decompiled"),
                ReferencesPath = Path.Combine(Program.EXEPath, "References"),
                Threads = 4
            };
            JsonSerializer serializer = new()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string path = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32S => WindowsDefault,
                PlatformID.Win32Windows => WindowsDefault,
                PlatformID.Win32NT => WindowsDefault,
                PlatformID.WinCE => WindowsDefault,

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
            { serializer.Serialize(jWriter, config); }

            Console.WriteLine($" Created a new configuration file in: {filePath}");

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
                // TODO: give extract, decompile, & references default values that don't save to the config for portability
                Threads = Program.Configuration.Threads
            }; 
            JsonSerializer serializer = new()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            using (StreamWriter writer = new(FilePath))
            using (JsonWriter jWriter = new JsonTextWriter(writer))
            { serializer.Serialize(jWriter, config); }

            Program.Configuration = JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(FilePath));
        }
    }
}
