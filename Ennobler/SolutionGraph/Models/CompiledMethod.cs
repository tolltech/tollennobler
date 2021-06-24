using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledMethod
    {
        public string Name => $"{Namespace}.{ClassName}.{ShortName}";
        public string ClassName { get; set; }
        public string ShortName { get; set; }
        public Compilation Compilation { get; set; }
        public ParameterSyntax[] Parameters { get; set; }
        public BlockSyntax MethodBody { get; set; }
        public SemanticModel SemanticModel { get; set; }
        public SyntaxTree DocumentSyntaxTree { get; set; }
        public string ProjectName { get; set; }
        public bool IsProperty { get; set; }
        public string Namespace { get; set; }
        public TypeSyntax ReturnType { get; set; }
        public string SourceCode { get; set; }
        public int StartLineNumber { get; set; }
        public int EndLineNumber { get; set; }
    }
}