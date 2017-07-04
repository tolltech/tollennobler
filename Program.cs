using System;
using System.Linq;
using log4net;
using Ninject;
using Tolltech.TollEnnobler.SolutionFixers;

namespace Tolltech.TollEnnobler
{
    class Program
    {
        private static ILog log = null;

        static void Main(string[] args)
        {
            try
            {
                var standardKernel = new StandardKernel(new ConfigurationModule("log4net.config"));

                log = LogManager.GetLogger(typeof(Program));

                var fixers = standardKernel.GetAll<IFixer>().OrderBy(x => x.Order).ToArray();
                var solutionProcessor = standardKernel.Get<ISolutionProcessor>();

                var success = solutionProcessor.Process(args[0], fixers);

                if (!success)
                    log.ToError($"Changes cant be applied!");
                else
                    log.ToConsole($"Changes was applied!");
            }
            catch (Exception ex)
            {
                if (log == null)
                    throw;

                log.Error($"Something goes wrong", ex);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
