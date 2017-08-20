using Tolltech.TollEnnobler;

namespace Tolltech.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionProcessor = new FixerRunner();

            solutionProcessor.Run(new Settings
            {
                Log4NetFileName = "log4net.config",
                ProjectNameFilter = x => x.Contains("Billing"),
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = "C:/_work/MegaWithoutCI.sln"
            });
        }
    }
}
