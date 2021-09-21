using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Tolltech.Common;
using Tolltech.Ennobler.Models;
using Tolltech.EnnoblerGraph.Metrics;

namespace Tolltech.EnnoblerGraph.Parsers
{
    public class MetricDataParser : IMetricDataParser
    {
        public MethodMetrics[] ParseXml(string inputFileName)
        {
            var orAdd = new XmlSerializer(typeof(CodeMetricsReport));

            using var stream = new FileStream(inputFileName, FileMode.Open);

            CodeMetricsReport report;
            using (var xmlTextReader = new XmlTextReader(stream) { XmlResolver = null })
            {
                report = (CodeMetricsReport)orAdd.Deserialize(xmlTextReader);
            }

            var metrics = new List<MethodMetrics>();
            foreach (var proj in report.Targets)
            foreach (var nameSpace in proj.Assembly.Namespaces)
            foreach (var type in nameSpace.Types)
            foreach (var member in type.Members)
            {
                metrics.Add(new MethodMetrics
                {
                    ClassCoupling = member.Metrics.First(x => x.Name == "ClassCoupling").Value,
                    CyclomaticComplexity = member.Metrics.First(x => x.Name == "CyclomaticComplexity").Value,
                    LinesOfExecutableCode = member.Metrics.First(x => x.Name == "ExecutableLines").Value,
                    LinesOfSourceCode = member.Metrics.First(x => x.Name == "SourceLines").Value,
                    MaintainabilityIndex = member.Metrics.First(x => x.Name == "MaintainabilityIndex").Value,
                    Name = new FullMethodName
                    {
                        NamespaceName = nameSpace.Name,
                        ClassName = type.Name,
                        MethodName = GetMethodName(member),
                        ParameterTypes = GetParametersFromCodeMetricsXmlReport(member.Name)
                    }
                });
            }

            return metrics.ToArray();
        }

        private string GetMethodName(Member member)
        {
            return new string(member.Name.SkipWhile(c => c != '.').Skip(1).TakeWhile(char.IsLetterOrDigit).ToArray());
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
                        ParameterTypes = GetParametersFromCodeMetricsCsvReport(splits[4])
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

        private static string[] GetParametersFromCodeMetricsCsvReport(string methodName)
        {
            //"BuildFiscalizationsHistory(IFiscalization[], FiscalizationBill, FiscalizationCheque[], FiscalizationBillMoney, Dictionary<string, FiscalizationCheque[]>, IFiscalization[]) : IBillFiscalizationHistory"

            if (methodName.Contains("FiscalizeAsync"))
            {
                var c = 2;
            }

            var underBraces = new string(methodName.SkipWhile(c => c != '(').TakeWhile(c => c != ')').ToArray());
            return underBraces.Trim('(', ')').Split(',').Select(x => x.Trim(' ')).ToArray();
        }

        private static string[] GetParametersFromCodeMetricsXmlReport(string methodName)
        {
            //"BuildFiscalizationsHistory(IFiscalization[], FiscalizationBill, FiscalizationCheque[], FiscalizationBillMoney, Dictionary<string, FiscalizationCheque[]>, IFiscalization[]) : IBillFiscalizationHistory"
            //todo: очевидно не работает с дженериками с запятыми, туплами с круглыми скобками и тд
            var underBraces = new string(methodName.SkipWhile(c => c != '(').TakeWhile(c => c != ')').ToArray());
            return underBraces.Trim('(', ')').Split(',').Select(x => x.Trim().Split(' ').First().Trim())
                .Where(x => x != string.Empty).ToArray();
        }
    }
}