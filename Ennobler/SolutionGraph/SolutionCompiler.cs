using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Tolltech.Ennobler.SolutionGraph.Helpers;
using Tolltech.Ennobler.SolutionGraph.Models;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler.SolutionGraph
{
    public class SolutionCompiler : ISolutionCompiler
    {
        private static readonly ILog log = LogProvider.Get().ForContext(typeof(SolutionCompiler));

        public async Task<CompiledSolution> CompileSolutionAsync(Solution solution)
        {
            var compiledProjectsList = new List<CompiledProject>();
            var solutionProjects = solution.Projects.ToArray();

            log.ToConsole($"Start compiling projects for {solution.FilePath}");

            var total = solutionProjects.Length;
            var iterator = 0;

            foreach (var project in solutionProjects)
            {
                if (project.SupportsCompilation)
                {

                    var compilation = project.GetCompilationAsync().Result;

                    compiledProjectsList.Add(new CompiledProject(project, compilation));
                }

                log.ToConsole($"Compiling projects {++iterator}/{total}");
            }

            log.ToConsole($"Finish compiling projects for {solution.FilePath}");

            var compiledProjectModel = new CompiledProjectsModel(compiledProjectsList.ToArray());
            await compiledProjectModel.FillCompilationAsync().ConfigureAwait(false);

            return new CompiledSolution
            {
                CompiledProjects = compiledProjectModel,
                Solution = solution
            };
        }
    }
}