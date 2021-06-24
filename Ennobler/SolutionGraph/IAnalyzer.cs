using System.Threading.Tasks;
using Tolltech.Ennobler.SolutionGraph.Models;

namespace Tolltech.Ennobler.SolutionGraph
{
    public interface IAnalyzer
    {
        Task AnalyzeAsync(CompiledSolution compiledSolution);
    }
}