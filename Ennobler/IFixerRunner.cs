using log4net;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.Ennobler
{
    public interface IFixerRunner
    {
        bool Run(ISettings settings, NinjectModule configurationModule = null);
        bool Run(ISettings settings, IFixer[] fixers, ILog configuredLog = null);
    }
}