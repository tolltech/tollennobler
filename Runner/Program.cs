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
                ProjectNameFilter = x => x.Contains("TestingCore"),
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = "D:/billy/MegaWithoutCI.sln",
                VisualStudioInstallationPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional",
                VisualStudioVersion = @"15.0"
            });
        }
    }
}
