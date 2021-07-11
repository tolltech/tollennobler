using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tolltech.Common;
using Tolltech.Ennobler.Models;
using Tolltech.Ennobler.SolutionGraph.Helpers;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledProjectsModel
    {
        private readonly ConcurrentDictionary<string, CompiledNamespaceModel> namespaces =
            new ConcurrentDictionary<string, CompiledNamespaceModel>();

        private ILookup<string, string> classesWithMethods;

        private static readonly ILog log = LogProvider.Get().ForContext(typeof(CompiledProjectsModel));

        private bool compilationFilled = false;

        public CompiledProjectsModel(CompiledProject[] compiledProjects)
        {
            CompiledProjects = compiledProjects;
            compilationsByProject = CompiledProjects.ToDictionary(x => x.Project, x => x.Compilation);
        }

        public async Task FillCompilationAsync()
        {
            log.ToConsole($"Start filling symbol tree.");

            var totalDocumentsCount = CompiledProjects.Sum(x => x.Project.Documents.Count());
            var documentIterator = 0;
            var currentPercent = 0;
            foreach (var compiledProject in CompiledProjects)
            {
                foreach (var document in compiledProject.Project.Documents)
                {
                    var newPercent = ++documentIterator * 10 / totalDocumentsCount;
                    if (newPercent > currentPercent)
                    {
                        currentPercent = newPercent;
                        log.ToConsole($"Filling {currentPercent * 10}% documents");
                    }

                    var documentRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
                    var documentSyntaxTree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
                    if (!document.SupportsSyntaxTree || documentRoot == null || documentSyntaxTree == null)
                    {
                        log.Error($"Document doesnt support syntax tree; {document.FilePath}");
                        continue;
                    }

                    var sourceText = documentRoot.GetText();
                    var semanticModel = compiledProject.Compilation.GetSemanticModel(documentSyntaxTree);
                    var classes = documentRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();

                    foreach (var classDeclarationSyntax in classes)
                    {
                        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);

                        if (classSymbol == null)
                        {
                            throw new Exception(
                                $"Can't create symbol for classDeclaration {classDeclarationSyntax.Identifier.Text}");
                        }

                        var fieldDeclarationSyntaxes = classDeclarationSyntax.DescendantNodes()
                            .OfType<FieldDeclarationSyntax>().ToArray();
                        var strangeFieldDeclarations = fieldDeclarationSyntaxes
                            .Where(x => !x.Declaration.Variables.Any() || x.Declaration.Variables.Count > 1).ToArray();
                        if (strangeFieldDeclarations.Any())
                        {
                            log.Error(
                                $"Strange FieldDeclarations without variables {string.Join(", ", strangeFieldDeclarations.Select(x => x))}; {document.FilePath} {classDeclarationSyntax.Identifier.ValueText}");
                        }

                        var fields = fieldDeclarationSyntaxes.Except(strangeFieldDeclarations).Select(x => new
                        {
                            Name = x.Declaration.Variables.First().Identifier.ValueText,
                            Parameters = new ParameterSyntax[0],
                            MethodBody = (SyntaxNode) null,
                            MemberType = MemberType.Field,
                            ReturnType = x.Declaration.Type,
                            Span = x.Span
                        }).ToArray();


                        if (classSymbol.Name == "FiscalizationSet")
                        {
                            var c = 0;
                        }

                        var propertyDeclarationSyntaxes = classDeclarationSyntax.DescendantNodes()
                            .OfType<PropertyDeclarationSyntax>().ToArray();
                        var properties = propertyDeclarationSyntaxes.Select(x => new
                        {
                            Name = semanticModel.GetDeclaredSymbol(x)?.Name,
                            Parameters = new ParameterSyntax[0],
                            MethodBody = (SyntaxNode)x.AccessorList?.Accessors.FirstOrDefault(y => y.Keyword.ValueText == "get")?.Body
                            ?? x.AccessorList?.Accessors.FirstOrDefault(y => y.Keyword.ValueText == "get")?.ExpressionBody
                            ?? x.ExpressionBody,
                            MemberType = MemberType.Property,
                            ReturnType = x.Type,
                            Span = x.Span
                        }).ToArray();

                        var methodDeclarationSyntaxes = classDeclarationSyntax.DescendantNodes()
                            .OfType<MethodDeclarationSyntax>().ToArray();
                        var methods = methodDeclarationSyntaxes.Select(x => new
                        {
                            Name = semanticModel.GetDeclaredSymbol(x)?.Name,
                            Parameters = x.ParameterList.Parameters.Select(y => y).ToArray(),
                            MethodBody = (SyntaxNode)x.Body,
                            MemberType = MemberType.Method,
                            ReturnType = x.ReturnType,
                            Span = x.Span
                        }).ToArray();


                        var ctorDeclarationSyntaxes = classDeclarationSyntax.DescendantNodes()
                            .OfType<ConstructorDeclarationSyntax>().ToArray();
                        var ctors = ctorDeclarationSyntaxes.Select(x => new
                        {
                            Name = classSymbol.Name,
                            Parameters = x.ParameterList.Parameters.Select(y => y).ToArray(),
                            MethodBody = (SyntaxNode)x.Body,
                            MemberType = MemberType.Ctor,
                            ReturnType = (TypeSyntax)null,
                            Span = x.Span
                        }).ToArray();

                        var className = classSymbol.Name;
                        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                        var newMethods = fields.Concat(properties).Concat(methods).Concat(ctors).Select(x => new CompiledMethod
                        {
                            Namespace = namespaceName,
                            ClassName = className,
                            ShortName = x.Name,
                            Compilation = compiledProject.Compilation,
                            Parameters = x.Parameters,
                            MethodBody = x.MethodBody,
                            SemanticModel = semanticModel,
                            DocumentSyntaxTree = documentSyntaxTree,
                            IsProperty = x.MemberType == MemberType.Property || x.MemberType == MemberType.Field,
                            ReturnType = x.ReturnType,
                            SourceCode = x.MethodBody?.Parent?.ToFullString() ?? "...",
                            StartLineNumber = sourceText.Lines.IndexOf(x.Span.Start) + 1,
                            EndLineNumber = sourceText.Lines.IndexOf(x.Span.End) + 1
                        }).ToList();

                        namespaces.AddOrUpdate(namespaceName,
                            ns => new CompiledNamespaceModel
                            {
                                Name = ns,
                                Classes = new Dictionary<string, CompiledClassModel>
                                {
                                    {
                                        className, new CompiledClassModel
                                        {
                                            Name = className,
                                            Methods = newMethods
                                        }
                                    }
                                }
                            },
                            (ns, namespaceModel) =>
                            {
                                if (namespaceModel.Classes.TryGetValue(className, out var compiledClass))
                                {
                                    compiledClass.Methods.AddRange(newMethods);
                                }
                                else
                                {
                                    namespaceModel.Classes[className] = new CompiledClassModel
                                    {
                                        Name = className,
                                        Methods = newMethods
                                    };
                                }

                                return namespaceModel;
                            });
                    }
                }
            }

            classesWithMethods = namespaces.SelectMany(x => x.Value.Classes.Values)
                .SelectMany(cl => cl.Methods.Select(m => (ClassName: cl.Name, MethodName: m.ShortName)))
                .ToLookup(x => x.ClassName, x => x.MethodName);

            log.ToConsole($"Symbol tree is filled.");
            compilationFilled = true;
        }

        private enum MemberType
        {
            Method,
            Property,
            Field,
            Ctor
        }

        [ItemNotNull]
        [NotNull]
        public CompiledMethod[] GetMethods(FullMethodName methodName)
        {
            if (!compilationFilled)
            {
                throw new Exception($"You should call {nameof(FillCompilationAsync)} before use {nameof(GetMethods)} method");
            }

            var parameterTypesStr = string.Join(",", methodName.ParameterTypes ?? Array.Empty<string>());
            return namespaces
                       .SafeGet(methodName.NamespaceName)?
                       .Classes.SafeGet(methodName.ClassName)?
                       .Methods
                       .Where(x => x.ShortName == methodName.MethodName)
                       .Where(x => string.IsNullOrWhiteSpace(parameterTypesStr) ||
                                   parameterTypesStr == string.Join(",", x.ParameterInfos.Select(p => p.Type)))
                       .ToArray()
                   ?? Array.Empty<CompiledMethod>();
        }

        public CompiledProject[] CompiledProjects { get; }

        private readonly Dictionary<Project, Compilation> compilationsByProject;

        public Compilation GetCompilationByProject(Project project)
        {
            return compilationsByProject.GetOrAdd(project, x => x.GetCompilationAsync().Result);
        }

        public bool HasMethod(string className, string methodName)
        {
            return classesWithMethods[className].Contains(methodName);
        }
    }
}