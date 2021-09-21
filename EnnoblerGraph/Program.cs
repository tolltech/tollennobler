using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tolltech.Common;
using Tolltech.Ennobler;
using Tolltech.Ennobler.Helpers;
using Tolltech.Ennobler.Models;
using Tolltech.Ennobler.SolutionGraph.Helpers;
using Tolltech.Ennobler.SolutionGraph.Models;
using Tolltech.EnnoblerGraph.Metrics;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Tolltech.EnnoblerGraph
{
    public static class Program
    {
        private static ILog log;

        public static async Task Main()
        {
            LogProvider.Configure(GetLog());
            log = GetLog();

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

            var codeMetricsByName =
                codeMetrics.ToLookup(x => (x.Name.NamespaceName, x.Name.ClassName, x.Name.MethodName));

            var runner = new Runner();

            var callStackBuilder = new CallStackBuilder();
            await runner.RunAnalyzersAsync(new Settings
            {
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = "C:\\work\\billy\\PG.sln",
            }, new[] {callStackBuilder}).ConfigureAwait(false);

            var entryPointMethodName = new FullMethodName
            {
                ClassName = "PaymentTransactionService",
                MethodName = "FiscalizeAsync",
                NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.ApplicationLayer",
                ParameterTypes = new[] {"KBAImportBillModel", "BillingSystem", "Guid?", "CashFlowDbo[]"}
            };

            WriteMetrics(callStackBuilder, codeMetricsByName, "Prefix", entryPointMethodName,
                new FullMethodName
                {
                    ClassName = "FiscalizationClassifier",
                    MethodName = "Classify",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.DomainLayer"
                });

            WriteMetrics(callStackBuilder, codeMetricsByName, "ShipmentError", entryPointMethodName,
                new FullMethodName
                {
                    ClassName = "FiscalizationContractStatesDetector",
                    MethodName = "InnerGetShipmentFiscalizationContractStates",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.DomainLayer"
                });

            WriteMetrics(callStackBuilder, codeMetricsByName, "Contract", entryPointMethodName,
                new FullMethodName
                {
                    ClassName = "FiscalizationContractDboConverter",
                    MethodName = "Convert",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.ApplicationLayer"
                });

            WriteMetrics(callStackBuilder, codeMetricsByName, "DoubleCheque", entryPointMethodName,
                new FullMethodName
                {
                    ClassName = "FiscalizationContractStatesDetector",
                    MethodName = "InnerGetFiscalizationContractStates",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.DomainLayer"
                });

            WriteMetrics(callStackBuilder, codeMetricsByName, "ShipmentCheque", entryPointMethodName,
                new FullMethodName
                {
                    ClassName = "BillFiscalizationsHistory",
                    MethodName = "GetFiscalizationsToCreate",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.DomainLayer"
                });

            WriteMetrics(callStackBuilder, codeMetricsByName, "OnlineClassify", entryPointMethodName,
                new FullMethodName
                {
                    ClassName = "Fiscalization",
                    MethodName = "NeedToBeFiscalizedByPayment",
                    NamespaceName = "SKBKontur.Billy.Billing.PaymentsGateway.Layers.DomainLayer"
                });


            //log.Info(sb.ToString());

            //sb.Clear();
            //foreach (var tree in nodes)
            //{
            //    var level = 0;
            //    foreach (var node in tree.RightWays())
            //    {
            //        var metricsStr = GetMetricsStr(codeMetricsByName, node, level++, log);
            //        sb.AppendLine(metricsStr);
            //    }
            //}

            //log.Info(sb.ToString());
        }

        private static void WriteMetrics(CallStackBuilder callStackBuilder,
            ILookup<(string NamespaceName, string ClassName, string MethodName), MethodMetrics> codeMetricsByName,
            string name, FullMethodName entryPoint, FullMethodName targetPoint)
        {
            var nodes = callStackBuilder.GetFullestCallStack(entryPoint, targetPoint, out var targetMethod);

            //var sb = new StringBuilder();
            //foreach (var tree in nodes)
            //foreach (var node in tree.Dfs())
            //{
            //    var metricsStr = GetMetricsStr(codeMetricsByName, node.Node, node.Level);

            //    sb.AppendLine(metricsStr);
            //}
            //log.Info(sb.ToString());

            foreach (var treeNode in nodes)
            {
                var maxMetrics = treeNode.Dfs()
                    .Select(x => GetMethodMetrics(codeMetricsByName, x.Node.Node))
                    .Where(x => x != null)
                    .ToArray();
                var minMetrics = treeNode.GetFastestWay(x =>
                        x.Name == targetMethod.Name && x.ParametersAreSuitable(targetMethod, out _))
                    .Select(x => GetMethodMetrics(codeMetricsByName, x))
                    .Where(x => x != null)
                    .ToArray();

                var aggregatedMaxMetrics = GetAggregatedMetrics(maxMetrics);
                var aggregatedMinMetrics = GetAggregatedMetrics(minMetrics);

                log.Warn($"{name} MAX - {aggregatedMaxMetrics}");
                log.Warn($"{name} MIN - {aggregatedMinMetrics}");
            }
        }

        private static (int MaxMaintainabilityIndex, int MinMaintainabilityIndex, double AvgMaintainabilityIndex, int
            MedMaintainabilityIndex, int MaxCyclomaticComplexity, int MinCyclomaticComplexity, double
            AvgCyclomaticComplexity,
            int MedCyclomaticComplexity, int LinesOfExecutableCode, int LinesOfSourceCode, int MaxClassCoupling, int
            MinClassCoupling, double AvgClassCoupling, int MedClassCoupling, int P95ClassCoupling) GetAggregatedMetrics(
                MethodMetrics[] metrics)
        {
            var aggregatedMaxMetrics = (
                MaxMaintainabilityIndex: metrics.Max(x => x.MaintainabilityIndex),
                MinMaintainabilityIndex: metrics.Min(x => x.MaintainabilityIndex),
                AvgMaintainabilityIndex: metrics.Average(x => x.MaintainabilityIndex),
                MedMaintainabilityIndex: metrics.Select(x => x.MaintainabilityIndex).ToArray().GetMedian(),
                MaxCyclomaticComplexity: metrics.Max(x => x.CyclomaticComplexity),
                MinCyclomaticComplexity: metrics.Min(x => x.CyclomaticComplexity),
                AvgCyclomaticComplexity: metrics.Average(x => x.CyclomaticComplexity),
                MedCyclomaticComplexity: metrics.Select(x => x.CyclomaticComplexity).ToArray().GetMedian(),
                LinesOfExecutableCode: metrics.Sum(x => x.LinesOfExecutableCode),
                LinesOfSourceCode: metrics.Sum(x => x.LinesOfSourceCode),
                MaxClassCoupling: metrics.Max(x => x.ClassCoupling),
                MinClassCoupling: metrics.Min(x => x.ClassCoupling),
                AvgClassCoupling: metrics.Average(x => x.ClassCoupling),
                MedClassCoupling: metrics.Select(x => x.ClassCoupling).ToArray().GetMedian(),
                P95ClassCoupling: metrics.Select(x => x.ClassCoupling).ToArray().GetPercentile(95)
            );
            return aggregatedMaxMetrics;
        }

        private static string GetMetricsStr(
            ILookup<(string NamespaceName, string ClassName, string MethodName), MethodMetrics> codeMetricsByName,
            TreeNode<CompiledMethod> node, int level)
        {
            var methodMetric = GetMethodMetrics(codeMetricsByName, node.Node);

            var metricsStr = new string('\t', level) + $"{node.Node.ClassName}" +
                             $".{node.Node.ShortName}" +
                             $".{string.Join(",", node.Node.ParameterInfos.Select(x => x.Type))}" +
                             $" - MI {methodMetric?.MaintainabilityIndex};Cyclo {methodMetric?.CyclomaticComplexity};Lines {methodMetric?.LinesOfExecutableCode};Coupling {methodMetric?.ClassCoupling}";
            return metricsStr;
        }

        private static MethodMetrics GetMethodMetrics(
            ILookup<(string NamespaceName, string ClassName, string MethodName), MethodMetrics> codeMetricsByName,
            CompiledMethod node)
        {
            var methodMetrics =
                codeMetricsByName[(node.Namespace, node.ClassName, node.ShortName)].ToArray();
            var suitableMethodMetrics = methodMetrics;

            var sourceParameters = node.ParameterInfos.Select(x => x?.Type).ToArray();
            if (suitableMethodMetrics.Length > 1)
            {
                suitableMethodMetrics = suitableMethodMetrics
                    .Where(x => SyntaxHelpers.ParametersAreSuitableSimple(sourceParameters, x.Name.ParameterTypes))
                    .ToArray();
            }

            if (suitableMethodMetrics.Length > 1)
            {
                log.Error(
                    $"Find several metrics for {node.Name} with parameters {string.Join(",", sourceParameters)}");
                $"Find several metrics,{node.Name},,{string.Join(",", sourceParameters)}".ToStructuredLogFile();
            }

            if (suitableMethodMetrics.Length == 0)
            {
                log.Error($"Find 0 metrics for {node.Name} with parameters {string.Join(",", sourceParameters)}");
                $"Find 0 metrics,{node.Name},,{string.Join(",", sourceParameters)}".ToStructuredLogFile();
            }

            var methodMetric = suitableMethodMetrics.FirstOrDefault();
            return methodMetric;
        }

        private static string[] GetParametersFromCodeMetricsReport(string methodName)
        {
            //"BuildFiscalizationsHistory(IFiscalization[], FiscalizationBill, FiscalizationCheque[], FiscalizationBillMoney, Dictionary<string, FiscalizationCheque[]>, IFiscalization[]) : IBillFiscalizationHistory"

            if (methodName.Contains("FiscalizeAsync"))
            {
                var c = 2;
            }

            var underBraces = new string(methodName.SkipWhile(c => c != '(').TakeWhile(c => c != ')').ToArray());
            return underBraces.Trim('(', ')').Split(',').Select(x => x.Trim(' ')).ToArray();
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
                ),
                new FileLog(
                    new FileLogSettings
                    {
                        FilePath = "logs/logWarn",
                        EnabledLogLevels = new [] {LogLevel.Warn},
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