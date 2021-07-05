using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tolltech.Common;
using Tolltech.Ennobler;
using Tolltech.Ennobler.Helpers;
using Tolltech.Ennobler.Models;
using Tolltech.Ennobler.SolutionGraph.Helpers;
using Tolltech.EnnoblerGraph.Metrics;
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
            var log = GetLog();

            var inputMetricsLines = File.ReadAllLines("input.csv");

            var headerLine = inputMetricsLines[0];
            if (headerLine !=
                "Scope\tProject\tNamespace\tType\tMember\tMaintainability Index\tCyclomatic Complexity\tDepth of Inheritance\tClass Coupling\tLines of Source code\tLines of Executable code")
            {
                throw new Exception("Inconsistent metrics file");
            }

            var codeMetrics = new List<MethodMetrics>(inputMetricsLines.Length - 1);

            foreach (var line in inputMetricsLines.Skip(1))
            {
                var splits = line.Split('\t');

                if (splits[0] != "Member")
                {
                    continue;
                }

                codeMetrics.Add(new MethodMetrics
                {
                    Name = new FullMethodName
                    {
                        ClassName = splits[3],
                        MethodName = new string(splits[4].Trim('"').TakeWhile(char.IsLetterOrDigit).ToArray()),
                        NamespaceName = splits[2],
                        ParameterTypes = GetParametersFromCodeMetricsReport(splits[4])
                    },
                    MaintainabilityIndex = splits[5].ToIntDefault(),
                    CyclomaticComplexity = splits[6].ToIntDefault(),
                    ClassCoupling = splits[8].ToIntDefault(),
                    LinesOfSourceCode = splits[9].ToIntDefault(),
                    LinesOfExecutableCode = splits[10].ToIntDefault()
                });
            }

            var codeMetricsByName = codeMetrics.ToLookup(x => (x.Name.NamespaceName, x.Name.ClassName, x.Name.MethodName));

            //var codeMetricsByName = codeMetrics.ToDictionary(x => )

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
                    MethodName = "FiscalizeAsync",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.ApplicationLayer",
                    ParameterTypes = new[] {"KBAImportBillModel", "BillingSystem", "Guid?", "CashFlowDbo[]"}
                },
                new FullMethodName
                {
                    ClassName = "PaymentTransactionService",
                    MethodName = "SendFiscalizationCountByBillMetrics",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.ApplicationLayer"
                });

            var sb = new StringBuilder();
            foreach (var tree in nodes)
            foreach (var node in tree.Dfs())
            {
                var methodMetrics = codeMetricsByName[(node.Node.Node.Namespace, node.Node.Node.ClassName, node.Node.Node.ShortName)].ToArray();
                var suitableMethodMetrics = methodMetrics;

                var sourceParameters = node.Node.Node.ParameterInfos.Select(x => x?.Type).ToArray();
                if (suitableMethodMetrics.Length > 1)
                {
                    suitableMethodMetrics = suitableMethodMetrics.Where(x => SyntaxHelpers.ParametersAreSuitableSimple(sourceParameters, x.Name.ParameterTypes)).ToArray();
                }

                if (suitableMethodMetrics.Length > 1)
                {
                    log.Error($"Find several metrics for {node.Node.Node.Name} with parameters {string.Join(",", sourceParameters)}");
                    $"Find several metrics,{node.Node.Node.Name},,{string.Join(",", sourceParameters)}".ToStructuredLogFile();
                }

                if (suitableMethodMetrics.Length == 0)
                {
                    log.Error($"Find 0 metrics for {node.Node.Node.Name} with parameters {string.Join(",", sourceParameters)}");
                    $"Find 0 metrics,{node.Node.Node.Name},,{string.Join(",", sourceParameters)}".ToStructuredLogFile();
                }

                var methodMetric = suitableMethodMetrics.FirstOrDefault();

                sb.AppendLine(new string('\t', node.Level) + $"{node.Node.Node.ClassName}" +
                              $".{node.Node.Node.ShortName}" +
                              $".{string.Join(",", node.Node.Node.ParameterInfos.Select(x => x.Type))}" +
                              $" - MI {methodMetric?.MaintainabilityIndex};Cyclo {methodMetric?.CyclomaticComplexity};Lines {methodMetric?.LinesOfExecutableCode};Coupling {methodMetric?.ClassCoupling}");
            }


            log.Info(sb.ToString());
            var ss = sb.ToString();
        }

        private static string[] GetParametersFromCodeMetricsReport(string methodName)
        {
            //"BuildFiscalizationsHistory(IFiscalization[], FiscalizationBill, FiscalizationCheque[], FiscalizationBillMoney, Dictionary<string, FiscalizationCheque[]>, IFiscalization[]) : IBillFiscalizationHistory"

            if (methodName.Contains("FiscalizeAsync"))
            {
                var c = 2;
            }

            var underBraces = new string(methodName.SkipWhile(c => c != '(').TakeWhile(c => c != ')').ToArray());
            return underBraces.Trim('(',')').Split(',').Select(x => x.Trim(' ')).ToArray();
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