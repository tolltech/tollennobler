using System.Threading.Tasks;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.Ennobler
{
    public interface IRunner
    {
        Task<bool> RunFixersAsync(ISettings settings, NinjectModule configurationModule = null);
        Task<bool> RunFixersAsync(ISettings settings, IFixer[] fixer);

        Task RunAnalyzersAsync(ISettings settings, NinjectModule configurationModule = null);
    }
}
