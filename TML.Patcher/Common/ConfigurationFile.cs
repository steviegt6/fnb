using System;
using System.IO;
using Newtonsoft.Json;

namespace TML.Patcher.Common
{
    public sealed class ConfigurationFile
    {
        [JsonIgnore]
        public const string UndefinedPath = "undefined";

        public static string FilePath { get; private set; }

        public string ModsPath { get; set; }

        public string ExtractPath { get; set; }

        public string DecompilePath { get; set; }

        public string ReferencesPath { get; set; }

        internal ConfigurationFile() { }

        public static ConfigurationFile Load(string filePath)
        {
            FilePath = filePath;

            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(filePath));

            Console.WriteLine(" Configuration file not found! Generating a new config.json file...");

            ConfigurationFile config = new()
            {
                ModsPath = UndefinedPath,
                ExtractPath = Path.Combine(Program.EXEPath, "Extracted"),
                DecompilePath = Path.Combine(Program.EXEPath, "Decompiled"),
                ReferencesPath = Path.Combine(Program.EXEPath, "References")
            };
            JsonSerializer serializer = new()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

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
                ModsPath = Program.Configuration.ModsPath,
                ExtractPath = Program.Configuration.ExtractPath,
                DecompilePath = Program.Configuration.DecompilePath,
                ReferencesPath = Program.Configuration.ReferencesPath
                // TODO: give extract, decompile, & references default values that don't save to the config for portability
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
