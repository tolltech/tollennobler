using System.Threading.Tasks;
using Tolltech.Ennobler;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Tolltech.EnnoblerGraph
{
    public static class Program
    {
        public static async Task Main()
        {
            LogProvider.Configure(GetLog());

            var runner = new Runner();

            await runner.RunAnalyzersAsync(new Settings
                {
                    RootNamespaceForNinjectConfiguring = "Tolltech",
                    SolutionPath = "C:\\work\\billy\\PG.sln",
                });
        }

        private static ILog GetLog()
        {
            return new CompositeLog(
                new SynchronousConsoleLog(new ConsoleLogSettings
                {
                    ColorsEnabled = true
                }),
                new FileLog(
                    new FileLogSettings
                    {
                        FilePath = "logs/log",
                        RollingStrategy = new RollingStrategyOptions
                        {
                            Type = RollingStrategyType.Hybrid,
                            Period = RollingPeriod.Day
                        }
                    }
                )
            );
        }
    }
}
