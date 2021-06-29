using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tolltech.Ennobler.Models;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledMethod
    {
        public string Name => $"{Namespace}.{ClassName}.{ShortName}";

        public FullMethodName FullMethodName => new FullMethodName
            {MethodName = ShortName, ClassName = ClassName, NamespaceName = Namespace};
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

        public string Hash => $"{Name}_{SerializeParametersInfo(GetMethodParametersInfos())}";

        private static string SerializeParametersInfo(MethodParameterInfo[] parameters)
        {
            return
                parameters == null
                    ? null
                    : $"{string.Join(";", parameters.Select(x => $"{x.Type},{x.Name},{string.Join("|", x.Modifiers)}"))}";
        }

        private MethodParameterInfo[] GetMethodParametersInfos()
        {
            return Parameters
                .Select(x => new MethodParameterInfo
                {
                    Type = x.Type?.ToString(),
                    Modifiers = x.Modifiers.Select(y => y.ValueText).ToArray(),
                    Name = x.Identifier.ValueText
                })
                .ToArray();
        }
    }
}