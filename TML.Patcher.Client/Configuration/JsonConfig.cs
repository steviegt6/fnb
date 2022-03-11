using System.ComponentModel;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using TML.Patcher.Client.Platform;

namespace TML.Patcher.Client.Configuration
{
    /// <summary>
    ///     Serializable JSON configuration file.
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    public abstract class JsonConfig<TConfig> where TConfig : JsonConfig<TConfig>, new()
    {
        /// <summary>
        ///     The automatic path.
        /// </summary>
        public static string Path => typeof(TConfig).GetCustomAttribute<DisplayNameAttribute>()!.DisplayName + ".json";

        /// <summary>
        ///     Serializes a config instance.
        /// </summary>
        public static void SerializeConfig(TConfig config, Storage storage)
        {
            File.WriteAllText(
                // If DisplayNameAttribute isn't able to be resolved then we have more problems than it just being absent.
                storage.GetFullPath(Path),
                JsonConvert.SerializeObject(config)
            );
        }

        /// <summary>
        ///     Deserializes the singleton config file.
        /// </summary>
        public static TConfig DeserializeConfig(Storage storage)
        {
            if (!storage.FileExists(Path))
                return new TConfig();

            return JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(storage.GetFullPath(Path))) ?? new TConfig();
        }
    }
}