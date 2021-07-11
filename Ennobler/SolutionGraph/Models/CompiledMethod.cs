using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Tolltech.Ennobler.Models;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    [DebuggerDisplay("{" + nameof(DebugName) + ",nq}")]
    public class CompiledMethod
    {
        public string DebugName => $"{ClassName}.{ShortName}.{string.Join(",", ParameterInfos.Select(x => x.Name))}";

        [JsonIgnore]
        public string Name => $"{Namespace}.{ClassName}.{ShortName}";

        [JsonIgnore]
        public FullMethodName FullMethodName => new FullMethodName {MethodName = ShortName, ClassName = ClassName, NamespaceName = Namespace};

        [JsonIgnore]
        public string Namespace { get; set; }

        [JsonIgnore]
        public string ClassName { get; set; }
        [JsonIgnore]
        public string ShortName { get; set; }
        [JsonIgnore]
        public MethodParameterInfo[] ParameterInfos => GetMethodParametersInfos();

        [JsonIgnore]
        public bool IsProperty { get; set; }
        [JsonIgnore]
        public int StartLineNumber { get; set; }
        [JsonIgnore]
        public int EndLineNumber { get; set; }

        [JsonIgnore]
        public TypeSyntax ReturnType { get; set; }
        [JsonIgnore]
        public string SourceCode { get; set; }
        [JsonIgnore]
        public Compilation Compilation { get; set; }
        [JsonIgnore]
        public ParameterSyntax[] Parameters { get; set; }
        [JsonIgnore]
        public SyntaxNode MethodBody { get; set; }
        [JsonIgnore]
        public SemanticModel SemanticModel { get; set; }
        [JsonIgnore]
        public SyntaxTree DocumentSyntaxTree { get; set; }

        [JsonIgnore]
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