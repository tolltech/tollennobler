using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Tolltech.Ennobler.SolutionGraph.Models;

namespace Tolltech.Ennobler.SolutionGraph
{
    public interface ISolutionCompiler
    {
        Task<CompiledSolution> CompileSolutionAsync(Solution solutionPath);
    }
}