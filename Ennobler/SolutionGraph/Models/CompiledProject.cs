using Microsoft.CodeAnalysis;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledProject
    {
        public CompiledProject(Project project, Compilation compilation)
        {
            Project = project;
            Compilation = compilation;
        }

        public Project Project { get; }
        public Compilation Compilation { get; }
    }
}