using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;

namespace Tolltech.Ennobler.SolutionFixers
{
    public class SolutionProcessor : ISolutionProcessor
    {
        private readonly ISettings settings;
        private static readonly ILog log = LogManager.GetLogger(typeof(SolutionProcessor));

        static SolutionProcessor()
        {
            MSBuildLocator.RegisterDefaults();
        }

        public SolutionProcessor(ISettings settings)
        {
            this.settings = settings;
        }

        public bool Process(string solutionPath, IFixer[] fixers)
        {
            using var msWorkspace = MSBuildWorkspace.Create(
                new Dictionary<string, string>
                {
                    {"BuildingInsideVisualStudio", "false"}
                }
            );

            msWorkspace.WorkspaceFailed += (sender, args) =>
                throw new Exception(
                    $"Fail to load Workspace with {args.Diagnostic.Kind} and message {args.Diagnostic.Message}");

            var solution = msWorkspace.OpenSolutionAsync(solutionPath).ConfigureAwait(false).GetAwaiter()
                .GetResult();

            var currentFixerIndex = 0;
            foreach (var fixer in fixers)
            {
                log.ToConsole($"Start fixer {fixer.Name}");
                Process(fixer, ref solution, ++currentFixerIndex, fixers.Length);
            }

            return solution.Workspace.TryApplyChanges(solution);
        }

        private void Process(IFixer fixer, ref Solution solution, int fixerIndex, int fixersCount)
        {
            var projectIds = solution.Projects
                .Where(x => settings.ProjectNameFilter?.Invoke(x.Name) ?? true)
                .Select(x => x.Id).ToArray();
            var currentProjectIndex = 0;
            foreach (var projectId in projectIds)
            {
                ++currentProjectIndex;

                var currentProject = solution.GetProject(projectId);

                log.ToConsole($"Project {currentProject.Name}");

                var currentDocumentIndex = 0;
                var documentIds = currentProject.Documents.Select(x => x.Id).ToArray();
                foreach (var documentId in documentIds)
                {
                    ++currentDocumentIndex;

                    var document = currentProject.GetDocument(documentId);
                    log.ToConsole(
                        $"{fixerIndex:00}/{fixersCount} - {currentProjectIndex:00}/{projectIds.Length} - {currentDocumentIndex:0000}/{documentIds.Length} // {currentProject.Name} // {document.Name}");
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
