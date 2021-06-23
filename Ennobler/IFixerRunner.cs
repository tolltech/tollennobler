using System.Threading.Tasks;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.Ennobler
{
    public interface IFixerRunner
    {
        Task<bool> RunAsync(ISettings settings, NinjectModule configurationModule = null);
        Task<bool> RunAsync(ISettings settings, IFixer[] fixer);
    }
}
