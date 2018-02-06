using System.Linq;
using log4net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;

namespace Tolltech.TollEnnobler.SolutionFixers
{
    public class SolutionProcessor : ISolutionProcessor
    {
        private readonly ISettings settings;
        private static readonly ILog log = LogManager.GetLogger(typeof(SolutionProcessor));

        public SolutionProcessor(ISettings settings)
        {
            this.settings = settings;
        }

        public bool Process(string solutionPath, IFixer[] fixers)
        {
            var msWorkspace = MSBuildWorkspace.Create();

            msWorkspace.WorkspaceFailed += (sender, args) => throw new Exception($"Fail to load Workspace with error {args.Diagnostic.Message} and kind {args.Diagnostic.Kind}");
            var solution = msWorkspace.OpenSolutionAsync(solutionPath).ConfigureAwait(false).GetAwaiter().GetResult();

            var f = 0;
            foreach (var fixer in fixers)
            {
                log.ToConsole($"Start fixer {fixer.Name}");
                Process(fixer, ref solution, ++f, fixers.Length);
            }

            return solution.Workspace.TryApplyChanges(solution);
        }

        private void Process(IFixer fixer, ref Solution solution, int f, int totalF)
        {
            var projectIds = solution.Projects
                .Where(x => settings.ProjectNameFilter?.Invoke(x.Name) ?? true)
                .Select(x => x.Id).ToArray();
            var p = 0;
            foreach (var projectId in projectIds)
            {
                ++p;


                var currentProject = solution.GetProject(projectId);
                log.ToConsole($"Project {currentProject.Name}");
                var d = 0;
                var documentIds = currentProject.Documents.Select(x => x.Id).ToArray();
                foreach (var documentId in documentIds)
                {
                    ++d;


                    var document = currentProject.GetDocument(documentId);
                    log.ToConsole($"{f:00}/{totalF} - {p:00}/{projectIds.Length} - {d:0000}/{documentIds.Length} // {currentProject.Name} // {document.Name}");
                    if (!document.SupportsSyntaxTree)
                        continue;

                    var documentEditor = DocumentEditor.CreateAsync(document).Result;

                    fixer.Fix(document, documentEditor);

                    var newDocument = documentEditor.GetChangedDocument();
                    var newProject = newDocument.Project;
                    currentProject = newProject;
                }

                solution = currentProject.Solution;
            }
        }
    }
}