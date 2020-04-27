using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tolltech.Ennobler.Helpers
{
    public class ClassMethodDeclarationSyntax
    {
        public ClassDeclarationSyntax ClassDeclaration { get; set; }
        public MethodDeclarationSyntax MethodDeclaration { get; set; }
    }
}