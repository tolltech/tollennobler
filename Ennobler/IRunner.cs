using System.Threading.Tasks;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;
using Tolltech.Ennobler.SolutionGraph;

namespace Tolltech.Ennobler
{
    public interface IRunner
    {
        Task<bool> RunFixersAsync(ISettings settings, NinjectModule configurationModule = null);
        Task<bool> RunFixersAsync(ISettings settings, IFixer[] fixers);

        Task RunAnalyzersAsync(ISettings settings, NinjectModule configurationModule = null);
        Task RunAnalyzersAsync(ISettings settings, IAnalyzer[] analyzers);
    }
}
