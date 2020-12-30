using System.Threading.Tasks;

namespace Tolltech.Ennobler.SolutionFixers
{
    public interface ISolutionProcessor
    {
        Task<bool> ProcessAsync(string solutionPath, IFixer[] fixers);
    }
}
