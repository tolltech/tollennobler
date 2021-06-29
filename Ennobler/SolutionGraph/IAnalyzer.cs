using System.Threading.Tasks;
using JetBrains.Annotations;
using Tolltech.Ennobler.SolutionGraph.Models;

namespace Tolltech.Ennobler.SolutionGraph
{
    public interface IAnalyzer
    {
        [NotNull] Task AnalyzeAsync([NotNull] CompiledSolution compiledSolution);
    }
}