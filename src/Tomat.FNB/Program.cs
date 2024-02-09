using System.Threading.Tasks;
using CliFx;

namespace Tomat.FNB;

internal static class Program {
    private static async Task<int> Main(string[] args) {
        return await new CliApplicationBuilder()
            .SetTitle("fnb")
            .SetVersion(typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown")
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync(args);
    }
}
