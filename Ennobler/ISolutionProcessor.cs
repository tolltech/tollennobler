using System.Threading.Tasks;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.Ennobler
{
    public interface ISolutionProcessor
    {
        Task<bool> ProcessAsync(string solutionPath, IFixer[] fixers);
        Task ProcessAsync(string solutionPath, string projectName, string entryPointMethodNames);
    }
}
