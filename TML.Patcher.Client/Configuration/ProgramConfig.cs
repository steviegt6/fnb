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

        [JsonProperty("threads")] [DefaultValue(8D)]
        public double Threads = 8D;

        public string GetStoragePath(bool? beta = null)
        {
            string path = StoragePath;

            if (beta ?? UseBeta)
                path = System.IO.Path.Combine(path, "Beta");

            return path;
        }
    }
}