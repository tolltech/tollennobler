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
                log.Info($"Loading solution {solutionPath}...");

                using var workspace = CreateWorkspace();

                var solution = await workspace.OpenSolutionAsync(solutionPath).ConfigureAwait(false);

                var changesApplied = true;
                var currentFixerIndex = 0;
                foreach (var fixer in fixers)
                {
                    log.Info($"Start fixer {fixer.Name}");
                    changesApplied = await ProcessAsync(workspace, fixer, solution, ++currentFixerIndex, fixers.Length) && changesApplied;
                }

                return changesApplied;
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
        }

        private static MSBuildWorkspace CreateWorkspace()
        {
            var workspace = MSBuildWorkspace.Create(
                new Dictionary<string, string>
                {
                    ["BuildingInsideVisualStudio"] = "false",
                    ["BuildInParallel"] = "true",
                    ["MaxCpuCount"] = (Environment.ProcessorCount / 2 + 1).ToString()
                }
            );

            workspace.WorkspaceFailed += (sender, args) =>
            {
                if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                {
                    var message = $"Fail to load Workspace with {args.Diagnostic.Kind} and message {args.Diagnostic.Message}";

                    workspaceLog.Error(message);
                    throw new Exception(message);
                }

                workspaceLog.Warn(args.Diagnostic.Message);
            };
            workspace.WorkspaceChanged += (sender, args) => { workspaceLog.Info(args.Kind.ToString("G")); };
            workspace.LoadMetadataForReferencedProjects = false;

            return workspace;
        }

        private async Task<bool> ProcessAsync(MSBuildWorkspace workspace, IFixer fixer, Solution solution, int fixerIndex, int fixersCount)
        {
            var projects = solution.Projects
                .Where(x => settings.ProjectNameFilter?.Invoke(x.Name) ?? true)
                .ToList();

            var changesApplied = true;
            var currentProjectIndex = 0;
            foreach (var project in projects)
            {
                ++currentProjectIndex;

                log.Info($"Project {project!.Name}");

                var documents = project.Documents.ToList();

                var currentDocumentIndex = 0;
                foreach (var document in documents)
                {
                    ++currentDocumentIndex;

                    log.Debug($"{fixerIndex:00}/{fixersCount:00} - {currentProjectIndex:000}/{projects.Count:000} - {currentDocumentIndex:0000}/{documents.Count:0000} // {project.Name} // {document!.FilePath}");
                    if (!document.SupportsSyntaxTree)
                    {
                        continue;
                    }

                    var documentEditor = await DocumentEditor.CreateAsync(document);

                    await fixer.FixAsync(document, documentEditor);

                    var newDocument = documentEditor.GetChangedDocument();

                    changesApplied = workspace.TryApplyChanges(newDocument.Project.Solution) && changesApplied;
                }
            }

            return changesApplied;
        }
    }
}
