using System.Threading.Tasks;
using CliFx;

namespace TML.Patcher;

public static class Program
{
    public static async Task<int> Main(string[] args) {
        return await new CliApplicationBuilder()
                    .AddCommandsFromThisAssembly()
                    .Build()
                    .RunAsync(args);
    }
}