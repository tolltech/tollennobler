﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Tolltech.Common;
using Tolltech.Ennobler.Helpers;
using Tolltech.Ennobler.Models;
using Tolltech.Ennobler.SolutionGraph;
using Tolltech.Ennobler.SolutionGraph.Helpers;
using Tolltech.Ennobler.SolutionGraph.Models;
using Vostok.Logging.Abstractions;

namespace Tolltech.EnnoblerGraph
{
    public class CallStackBuilder : IAnalyzer
    {
        [NotNull] private CompiledSolution compiledSolution;

        private static readonly ILog log = LogProvider.Get().ForContext(typeof(CallStackBuilder));

        public Task AnalyzeAsync(CompiledSolution compiledSolution)
        {
            this.compiledSolution = compiledSolution;

            return Task.CompletedTask;
        }

        [NotNull]
        public TreeNode<CompiledMethod>[] GetFullestCallStack(FullMethodName entryPointMethodName,
            FullMethodName targetMethodName)
        {
            var entryPointMethods = compiledSolution.CompiledProjects.GetMethods(entryPointMethodName);
            var targetMethods = compiledSolution.CompiledProjects.GetMethods(targetMethodName);
            if (targetMethods.Length > 1)
            {
                throw new Exception($"More than 1 target method is not supported");
            }

            var nodes = new List<TreeNode<CompiledMethod>>();

            foreach (var entryPointMethod in entryPointMethods)
            {
                var node = BuildNode(entryPointMethod, targetMethods.Single());

                nodes.Add(node);
            }

            return nodes.ToArray();
        }

        private TreeNode<CompiledMethod> BuildNode([NotNull] CompiledMethod currentMethod, [NotNull] CompiledMethod targetMethod, HashSet<string> visitedMethodHashes = null)
        {
            visitedMethodHashes ??= new HashSet<string>();

            visitedMethodHashes.Add(currentMethod.Hash);

            var node = new TreeNode<CompiledMethod>
            {
                Node = currentMethod
            };

            if (currentMethod.FullMethodName.Equals(targetMethod.FullMethodName) &&
                currentMethod.ParametersAreSuitable(targetMethod, out _))
            {
                return node;
            }

            var childNodes = new List<TreeNode<CompiledMethod>>();
            var descendantNodes = currentMethod.MethodBody?.DescendantNodes();
            if (descendantNodes == null)
            {
                return node;
            }

            foreach (var syntaxNode in descendantNodes)
            {
                var foundMethod = FoundMethodsBySyntaxNode(syntaxNode, currentMethod);

                if (foundMethod == null || visitedMethodHashes.Contains(foundMethod.Hash))
                {
                    continue;
                }

                childNodes.Add(BuildNode(foundMethod, targetMethod));
            }

            node.Children = childNodes.ToArray();
            return node;
        }

        [CanBeNull]
        private CompiledMethod FoundMethodsBySyntaxNode(SyntaxNode syntaxNode, CompiledMethod compiledMethod)
        {
            var methodSearchParameters = TryGetDefault(syntaxNode, compiledMethod);

            if (methodSearchParameters == null)
            {
                return null;
            }

            var classSymbolWithBaseTypes = methodSearchParameters
                .Classes
                .Concat(
                    methodSearchParameters
                        .Classes
                        .SelectMany(x => x.GetBaseClasses()))
                .ToArray();

            var suitableMethodsByName = new List<CompiledMethod>(classSymbolWithBaseTypes.Length);
            foreach (var classSymbol in classSymbolWithBaseTypes)
            {
                if (classSymbol == null)
                {
                    continue;
                }

                if (classSymbol.ContainingNamespace.Name.StartsWith("System."))
                {
                    continue;
                }

                var fullMethodName = classSymbol.GetFullMethodName(methodSearchParameters.MethodName);
                suitableMethodsByName.AddRange(compiledSolution.CompiledProjects.GetMethods(fullMethodName));
            }

            var methodName = methodSearchParameters.MethodName;
            var parameters = methodSearchParameters.Parameters;
            var filePaths = string.Join(",", classSymbolWithBaseTypes.Select(x => $"{x.ContainingNamespace.ToDisplayString()}.{x.Name}"));
            if (suitableMethodsByName.Count == 0)
            {
                log.Error($"Find 0 candidates {methodName} with parameters {string.Join(",", parameters.Select(x => x?.ToString() ?? "null"))}");
                $"Find 0 candidates,{methodName},{filePaths},{string.Join(";", parameters.Select(x => x?.ToString() ?? "null"))}".ToStructuredLogFile();
               return null;
            }

            var suitableMethod = suitableMethodsByName.First();
            if (suitableMethodsByName.Count > 1)
            {
                var candidates = suitableMethodsByName.Select(x =>
                    {
                        var suitable = x.ParametersAreSuitable(parameters, out var identity);
                        return new { Suitable = suitable, Identity = identity, Candidate = x };
                    })
                    .Where(x => x.Suitable)
                    .OrderBy(x => x.Identity.HasValue && x.Identity.Value ? 0 : 1)
                    .Select(x => x.Candidate)
                    .ToArray();

                if (candidates.Length == 0)
                {
                    log.Error($"Find 0 methods {methodName} in document {filePaths} with parameters {string.Join(",", parameters.Select(x => x?.ToString() ?? "null"))}");
                    $"Find 0 methods,{methodName},{filePaths},{string.Join(";", parameters.Select(x => x?.ToString() ?? "null"))}".ToStructuredLogFile();
                }

                if (candidates.Length > 1)
                {
                    log.Error($"Find more than 1 method {methodName} in document {filePaths} with parameters {string.Join(",", parameters.Select(x => x?.ToString() ?? "null"))}");
                    $"Find more than 1 method,{methodName},{filePaths},{string.Join(";", parameters.Select(x => x?.ToString() ?? "null"))}".ToStructuredLogFile();
                }

                if (candidates.Length >= 1)
                {
                    suitableMethod = candidates.First();
                }
            }

            return suitableMethod;
        }

        private MethodSearchParameters TryGetDefault(SyntaxNode syntaxNode, CompiledMethod compiledMethod)
        {
            var invocationExpressionSyntax = syntaxNode as InvocationExpressionSyntax;
            if (invocationExpressionSyntax == null)
            {
                return null;
            }

            var memberAccessExpressionSyntax = invocationExpressionSyntax.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax == null)
            {
                return null;
            }

            var methodName = memberAccessExpressionSyntax.Name.Identifier.ValueText;

            var fieldType = compiledMethod.SemanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression).Type;

            if (fieldType?.ContainingNamespace.ToString().StartsWith("System") ?? false)
            {
                return null;
            }

            if (fieldType == null)
            {
                log.Error($"cant get symbol of member {methodName} of document {syntaxNode.SyntaxTree.FilePath}");
                $"cant get symbol,{methodName},{syntaxNode.SyntaxTree.FilePath}".ToStructuredLogFile();
                return null;
            }

            var fieldImplementations = fieldType.TypeKind == TypeKind.Interface || fieldType.IsAbstract
                ? SymbolFinder.FindImplementationsAsync(fieldType, compiledSolution.Solution).Result.Cast<ITypeSymbol>()
                    .ToArray()
                : new[] {fieldType};

            var parameters = invocationExpressionSyntax.ArgumentList.Arguments
                .Select(x => compiledMethod.SemanticModel.GetTypeInfo(x.Expression).Type)
                .ToArray();

            return new MethodSearchParameters
            {
                MethodName = methodName,
                Parameters = parameters,
                Classes = fieldImplementations
            };
        }
    }
}