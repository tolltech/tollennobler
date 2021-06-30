using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tolltech.Ennobler;
using Tolltech.Ennobler.Models;
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

            var callStackBuilder = new CallStackBuilder();
            await runner.RunAnalyzersAsync(new Settings
            {
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = "C:\\work\\billy\\PG.sln",
            }, new[] {callStackBuilder});

            var nodes = callStackBuilder.GetFullestCallStack(
                new FullMethodName
                {
                    ClassName = "PaymentTransactionService",
                    MethodName = "SendChequesByTransactionsAsync",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.ApplicationLayer"
                },
                new FullMethodName
                {
                    ClassName = "PaymentTransactionService",
                    MethodName = "SendChequesByContextAsync",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.ApplicationLayer"
                });

            var flatten = nodes.SelectMany(x => x.Flatten()).ToArray();
            var nodesJson = JsonConvert.SerializeObject(nodes);
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