namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class SourceCodeInfo
    {
        public string SourceCode { get; set; }
        public int StartLineNumber { get; set; }
        public int EndLineNumber { get; set; }

        private static readonly SourceCodeInfo empty = new SourceCodeInfo
        {
            SourceCode = "Sorry, it is impossible to find this peace of sources!", StartLineNumber = 0,
            EndLineNumber = 0
        };

        public static SourceCodeInfo Empty => empty;
    }
}