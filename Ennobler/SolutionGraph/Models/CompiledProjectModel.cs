using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tolltech.Common;
using Tolltech.Ennobler.SolutionGraph.Helpers;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledProjectModel
    {
        private readonly Dictionary<string, Document> documentsByClassSymbolString = new Dictionary<string, Document>();
        private bool documentsWereFilled = false;

        public CompiledProjectModel(CompiledProject[] compiledProjects)
        {
            CompiledProjects = compiledProjects;
            compilationsByProject = CompiledProjects.ToDictionary(x => x.Project, x => x.Compilation);
        }

        public async Task FillDocumentsAsync()
        {
            foreach (var compiledProject in CompiledProjects)
            {
                foreach (var document in compiledProject.Project.Documents)
                {
                    var model = compiledProject.Compilation.GetSemanticModel(await document.GetSyntaxTreeAsync().ConfigureAwait(false));
                    var classes = (await document.GetSyntaxRootAsync().ConfigureAwait(false)).DescendantNodes()
                        .OfType<ClassDeclarationSyntax>();
                    foreach (var classDeclarationSyntax in classes)
                    {
                        var symbol = model.GetDeclaredSymbol(classDeclarationSyntax);
                        documentsByClassSymbolString[symbol.ToString().KillGenericTags()] = document;
                    }
                }
            }

            documentsWereFilled = true;
        }

        public CompiledProject[] CompiledProjects { get; }

        private readonly Dictionary<Project, Compilation> compilationsByProject;

        public Compilation GetCompilationByProject(Project project)
        {
            return compilationsByProject.GetOrAdd(project, x => x.GetCompilationAsync().Result);
        }

        public Document GetDocumentsCandidate(string classSymbolString)
        {
            if (!documentsWereFilled)
            {
                throw new Exception($"Call {nameof(FillDocumentsAsync)} before this method calling");
            }

            return documentsByClassSymbolString.TryGetValue(classSymbolString.KillGenericTags(), out var result)
                ? result
                : null;
        }
    }
}