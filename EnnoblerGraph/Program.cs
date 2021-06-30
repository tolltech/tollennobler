using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tolltech.Common;
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

            var sb = new StringBuilder();
            foreach (var node in nodes.Single().Dfs())
            {
                sb.AppendLine(new string('\t', node.Level) + $"{node.Node.Node.ClassName}" +
                              $".{node.Node.Node.ShortName}" +
                              $".{string.Join(",", node.Node.Node.ParameterInfos.Select(x => x.Type))}");
            }

            var ss = sb.ToString();
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