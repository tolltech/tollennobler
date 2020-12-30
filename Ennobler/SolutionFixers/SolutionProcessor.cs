using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler.SolutionFixers
{
    public class SolutionProcessor : ISolutionProcessor
    {
        private readonly ISettings settings;
        private static readonly ILog log = LogProvider.Get().ForContext(typeof(SolutionProcessor));
        private static readonly ILog workspaceLog = LogProvider.Get().ForContext(typeof(MSBuildWorkspace));

        static SolutionProcessor()
        {
            MSBuildLocator.RegisterDefaults();
        }

        public SolutionProcessor(ISettings settings)
        {
            this.settings = settings;
        }

        public async Task<bool> ProcessAsync(string solutionPath, IFixer[] fixers)
        {
            try
            {
                using var msWorkspace = MSBuildWorkspace.Create(
                    new Dictionary<string, string>
                    {
                        {"BuildingInsideVisualStudio", "false"}
                    }
                );

                msWorkspace.WorkspaceFailed += (sender, args) =>
                {
                    if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                    {
                        var message = $"Fail to load Workspace with {args.Diagnostic.Kind} and message {args.Diagnostic.Message}";

                        workspaceLog.Error(message);
                        throw new Exception(message);
                    }

                    workspaceLog.Warn(args.Diagnostic.Message);
                };
                msWorkspace.WorkspaceChanged += (sender, args) =>
                {
                    workspaceLog.Info(args.Kind.ToString("G"));
                };

                var solution = await msWorkspace.OpenSolutionAsync(solutionPath).ConfigureAwait(false);

                var currentFixerIndex = 0;
                foreach (var fixer in fixers)
                {
                    log.Info($"Start fixer {fixer.Name}");
                    await ProcessAsync(fixer, solution, ++currentFixerIndex, fixers.Length);
                }

                return solution.Workspace.TryApplyChanges(solution);
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
        }

        private async Task ProcessAsync(IFixer fixer, Solution solution, int fixerIndex, int fixersCount)
        {
            var projectIds = solution.Projects
                .Where(x => settings.ProjectNameFilter?.Invoke(x.Name) ?? true)
                .Select(x => x.Id).ToArray();
            var currentProjectIndex = 0;
            foreach (var projectId in projectIds)
            {
                ++currentProjectIndex;

                var currentProject = solution.GetProject(projectId);

                log.Info($"Project {currentProject.Name}");

                var currentDocumentIndex = 0;
                var documentIds = currentProject.Documents.Select(x => x.Id).ToArray();
                foreach (var documentId in documentIds)
                {
                    ++currentDocumentIndex;

                    var document = currentProject.GetDocument(documentId);
                    log.Info(
                        $"{fixerIndex:00}/{fixersCount} - {currentProjectIndex:00}/{projectIds.Length} - {currentDocumentIndex:0000}/{documentIds.Length} // {currentProject.Name} // {document.Name}");
                    if (!document.SupportsSyntaxTree)
                        continue;

                    var documentEditor = await DocumentEditor.CreateAsync(document);

                    await fixer.FixAsync(document, documentEditor);

                    var newDocument = documentEditor.GetChangedDocument();
                    var newProject = newDocument.Project;
                    currentProject = newProject;
                }

                //solution = currentProject.Solution;
            }
        }
    }
}
