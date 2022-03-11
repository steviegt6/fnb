using System.ComponentModel;
using Newtonsoft.Json;

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

        [JsonProperty("useModLoaderBeta")] [DefaultValue(false)]
        public bool UseBeta;

        public string GetStoragePath()
        {
            string path = StoragePath;

            if (UseBeta)
                path = System.IO.Path.Combine(path, "Beta");

            return path;
        }
    }
}