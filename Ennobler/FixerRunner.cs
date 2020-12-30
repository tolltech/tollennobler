using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Ninject;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.Ennobler
{
    public class FixerRunner : IFixerRunner
    {
        private static ILog log;

        public Task<bool> RunAsync(ISettings settings, NinjectModule configurationModule = null)
        {
            var standardKernel = new StandardKernel(configurationModule ?? new ConfigurationModule(settings));
            var fixers = standardKernel.GetAll<IFixer>().OrderBy(x => x.Order).ToArray();
            var solutionProcessor = standardKernel.Get<ISolutionProcessor>();

            return RunCoreAsync(settings, fixers, solutionProcessor, LogManager.GetLogger(typeof(FixerRunner)));
        }

        public Task<bool> RunAsync(ISettings settings, IFixer[] fixers, ILog configuredLog = null)
        {
            return RunCoreAsync(settings, fixers, new SolutionProcessor(settings), configuredLog);
        }

        private static async Task<bool> RunCoreAsync(
            ISettings settings,
            IFixer[] fixers,
            ISolutionProcessor solutionProcessor,
            ILog configuredLog
        )
        {
            try
            {
                log = configuredLog ?? LogManager.GetLogger(typeof(FixerRunner));

                var success = await solutionProcessor.ProcessAsync(settings.SolutionPath, fixers);

                if (!success)
                {
                    log?.ToError("Unable to apply changes");
                }
                else
                {
                    log?.ToConsole($"Changes were applied!");
                }

                return success;
            }
            catch (Exception ex)
            {
                if (log == null)
                {
                    throw;
                }

                log.Error("Something goes wrong", ex);
                Console.WriteLine(ex.Message);

                return false;
            }
        }
    }
}
