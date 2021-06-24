using Microsoft.CodeAnalysis;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledSolution
    {
        public CompiledProjectsModel CompiledProjects { get; set; }
        public Solution Solution { get; set; }
    }
}