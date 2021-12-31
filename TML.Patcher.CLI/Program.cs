using System.Threading.Tasks;
using CliFx;

namespace TML.Patcher.CLI
{
    /// <summary>
    ///     Entry-point and main launch handler.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The current program runtime.
        /// </summary>
        public static Runtime? Runtime { get; private set; }
        public static async Task<int> Main()
        {
            Runtime = new Runtime();

            return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync();
        }
    }
}