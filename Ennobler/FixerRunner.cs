using System;
using System.Linq;
using log4net;
using Ninject;
using Ninject.Modules;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.Ennobler
{
    public class FixerRunner : IFixerRunner
    {
        private static ILog log = null;

        public bool Run(ISettings settings, NinjectModule configurationModule = null)
        {
            try
            {
                var standardKernel = new StandardKernel(configurationModule ?? new ConfigurationModule(settings));

                log = LogManager.GetLogger(typeof(FixerRunner));

                var fixers = standardKernel.GetAll<IFixer>().OrderBy(x => x.Order).ToArray();
                var solutionProcessor = standardKernel.Get<ISolutionProcessor>();

                var success = solutionProcessor.Process(settings.SolutionPath, fixers);

                if (!success)
                    log?.ToError($"Changes cant be applied!");
                else
                    log?.ToConsole($"Changes was applied!");

                return success;
            }
            catch (Exception ex)
            {
                if (log == null)
                    throw;

                log.Error($"Something goes wrong", ex);
                Console.WriteLine(ex.Message);

                return false;
            }
        }

        public bool Run(ISettings settings, IFixer[] fixers, ILog configuredLog = null)
        {
            try
            {
                var solutionProcessor = new SolutionProcessor(settings);

                log = configuredLog ?? LogManager.GetLogger(typeof(FixerRunner));

                var success = solutionProcessor.Process(settings.SolutionPath, fixers);

                if (!success)
                    log?.ToError($"Changes cant be applied!");
                else
                    log?.ToConsole($"Changes was applied!");

                return success;
            }
            catch (Exception ex)
            {
                if (log == null)
                    throw;

                log.Error($"Something goes wrong", ex);
                Console.WriteLine(ex.Message);

                return false;
            }
        }
    }
}