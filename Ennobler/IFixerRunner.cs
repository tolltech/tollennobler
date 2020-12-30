using System.Threading.Tasks;
using log4net;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.Ennobler
{
    public interface IFixerRunner
    {
        Task<bool> RunAsync(ISettings settings, NinjectModule configurationModule = null);
        Task<bool> RunAsync(ISettings settings, IFixer[] fixers, ILog configuredLog = null);
    }
}
