using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tolltech.Ennobler.Helpers
{
    public static class DocumentExtensions
    {
        public static IEnumerable<MethodDeclarationSyntax> GetMethodDeclarations(this Document document)
        {
            var documentSyntaxTree = document.GetSyntaxTreeAsync().Result;
            var classDeclarationSyntaxes = documentSyntaxTree.GetRoot().DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var methodDesclarationSyntaxes =
                    classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
                var methodSyntaxNodes =
                    methodDesclarationSyntaxes.ToArray();

                foreach (var methodSyntaxNode in methodSyntaxNodes)
                {
                    yield return methodSyntaxNode;
                }
            }
        }

        public static IEnumerable<ClassMethodDeclarationSyntax> GetClassMethodDeclarations(this Document document)
        {
            var documentSyntaxTree = document.GetSyntaxTreeAsync().Result;
            var classDeclarationSyntaxes = documentSyntaxTree.GetRoot().DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var methodDesclarationSyntaxes =
                    classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
                var methodSyntaxNodes =
                    methodDesclarationSyntaxes.ToArray();

                foreach (var methodSyntaxNode in methodSyntaxNodes)
                {
                    yield return new ClassMethodDeclarationSyntax
                    {
                        ClassDeclaration = classDeclarationSyntax,
                        MethodDeclaration = methodSyntaxNode
                    };
                }
            }

        }
    }
}