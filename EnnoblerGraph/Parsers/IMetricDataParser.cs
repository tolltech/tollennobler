using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Tolltech.Common;
using Tolltech.Ennobler.Models;
using Tolltech.EnnoblerGraph.Metrics;

namespace Tolltech.EnnoblerGraph.Parsers
{
    public interface IMetricDataParser
    {
        [NotNull] [ItemNotNull] MethodMetrics[] ParseXml([NotNull] string inputFileName);
        [NotNull] [ItemNotNull] MethodMetrics[] ParseCsv([NotNull] string inputFileName);
    }

    public class MetricDataParser : IMetricDataParser
    {
        public MethodMetrics[] ParseXml(string inputFileName)
        {
            throw new System.NotImplementedException();
        }

        public MethodMetrics[] ParseCsv(string inputFileName)
        {
            var inputMetricsLines = File.ReadAllLines(inputFileName);

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

            return codeMetrics.ToArray();
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

    }
}