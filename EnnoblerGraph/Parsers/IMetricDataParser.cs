using JetBrains.Annotations;
using Tolltech.EnnoblerGraph.Metrics;

namespace Tolltech.EnnoblerGraph.Parsers
{
    public interface IMetricDataParser
    {
        [NotNull] [ItemNotNull] MethodMetrics[] ParseXml([NotNull] string inputFileName);
        [NotNull] [ItemNotNull] MethodMetrics[] ParseCsv([NotNull] string inputFileName);
    }
}