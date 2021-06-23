using System.Threading.Tasks;
using Tolltech.Ennobler;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Tolltech.EnnoblerRunner
{
    public static class Program
    {
        public static async Task Main()
        {
            LogProvider.Configure(GetLog());

            var fixerRunner = new FixerRunner();

            await fixerRunner.RunAsync(new Settings
                {
                    Log4NetFileName = "log4net.config",
                    RootNamespaceForNinjectConfiguring = "Tolltech",
                    SolutionPath = "C:\\_work\\billy\\Mega.sln",
                },
                new[]
                {
                    new DummyFixer()
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
