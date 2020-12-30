using System.Threading.Tasks;
using Tolltech.Ennobler;

namespace Tolltech.EnnoblerRunner
{
    public static class Program
    {
        public static async Task Main()
        {
            var fixerRunner = new FixerRunner();

            await fixerRunner.RunAsync(new Settings
                {
                    Log4NetFileName = "log4net.config",
                    RootNamespaceForNinjectConfiguring = "Tolltech",
                    SolutionPath = "C:\\_work\\billy\\Mega.sln",
                },
                new[]
                {
                    new DummyFixer()
                });
        }
    }
}
