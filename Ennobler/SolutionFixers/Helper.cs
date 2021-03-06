﻿using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.Ennobler.SolutionFixers
{
    public static class Helper
    {
        public static void AddUsingIfDoesntExists(this DocumentEditor documentEditor, string usingname)
        {
            var root = documentEditor.GetChangedDocument().GetSyntaxTreeAsync().Result.GetRoot();
            var usingStatements = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();

            if (usingStatements.Length == 0)
                return;

            if (usingStatements.All(x => x.Name.ToString() != usingname))
            {
                var usingFluientStatment = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(usingname)).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.Whitespace("\r\n"));
                documentEditor.InsertAfter(usingStatements.Last(), usingFluientStatment);
            }
        }
    }
}