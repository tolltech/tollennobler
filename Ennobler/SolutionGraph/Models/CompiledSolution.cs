using Microsoft.CodeAnalysis;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledSolution
    {
        public CompiledProjectModel CompiledProjects { get; set; }
        public Solution Solution { get; set; }
    }
}