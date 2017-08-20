using Ninject.Modules;

namespace Tolltech.TollEnnobler
{
    public interface IFixerRunner
    {
        bool Run(ISettings settings, NinjectModule configurationModule = null);
    }
}