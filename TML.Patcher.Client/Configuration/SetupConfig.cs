using System.ComponentModel;
using Newtonsoft.Json;

// no xml comments
#pragma warning disable 1591

namespace TML.Patcher.Client.Configuration
{
    /// <summary>
    ///     JSON-powered initial setup configuration class.
    /// </summary>
    [DisplayName("setupConfig")]
    public sealed class SetupConfig : JsonConfig<SetupConfig>
    {
        [JsonProperty("setupCompleted")] public bool SetupCompleted;
    }
}