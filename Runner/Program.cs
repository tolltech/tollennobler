using Tolltech.TollEnnobler;

namespace Tolltech.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var fixerRunner = new FixerRunner();

            fixerRunner.Run(new Settings
            {
                Log4NetFileName = "log4net.config",
                ProjectNameFilter = x => x.Contains("Payments"),
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = "D:/billy/MegaWithoutCI.sln",
            });
        }
    }
}
