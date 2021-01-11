using System;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler
{
    public class FixerRunner : IFixerRunner
    {
        private static readonly ILog log = LogProvider.Get().ForContext<FixerRunner>();

        public Task<bool> RunAsync(ISettings settings, NinjectModule configurationModule = null)
        {
            var standardKernel = new StandardKernel(configurationModule ?? new ConfigurationModule(settings));
            var fixers = standardKernel.GetAll<IFixer>().OrderBy(x => x.Order).ToArray();
            var solutionProcessor = standardKernel.Get<ISolutionProcessor>();

            return RunCoreAsync(settings, fixers, solutionProcessor);
        }

        public Task<bool> RunAsync(ISettings settings, IFixer[] fixers)
        {
            return RunCoreAsync(settings, fixers, new SolutionProcessor(settings));
        }

        private static async Task<bool> RunCoreAsync(
            ISettings settings,
            IFixer[] fixers,
            ISolutionProcessor solutionProcessor
        )
        {
            try
            {
                var success = await solutionProcessor.ProcessAsync(settings.SolutionPath, fixers);

                if (!success)
                {
                    log.Error("Some changes weren't applied");
                }
                else
                {
                    log.Info("All changes were applied!");
                }

                return success;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Something went wrong");

                return false;
            }
        }
    }
}
