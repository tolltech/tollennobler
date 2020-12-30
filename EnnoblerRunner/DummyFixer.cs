using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Tolltech.Ennobler.Helpers;
using Tolltech.Ennobler.SolutionFixers;

namespace Tolltech.EnnoblerRunner
{
    public class DummyFixer : IFixer
    {
        public string Name => "FixEntryPoint";

        public int Order => 0;

        public async Task FixAsync(Document document, DocumentEditor documentEditor)
        {
            var list = (await document.GetSyntaxRootAsync())!.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().ToList();
        }
    }
}
