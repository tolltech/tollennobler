using Tolltech.Ennobler.Models;

namespace Tolltech.EnnoblerGraph.Metrics
{
    public class MethodMetrics
    {
        public FullMethodName Name { get; set; }
        public int MaintainabilityIndex { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int ClassCoupling { get; set; }
        public int LinesOfSourceCode { get; set; }
        public int LinesOfExecutableCode { get; set; }
    }
}