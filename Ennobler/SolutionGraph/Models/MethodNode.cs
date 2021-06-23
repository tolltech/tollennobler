using System.Linq;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class MethodNode
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string ShortName { get; set; }
        public bool EntryPoint { get; set; }
        public bool Async { get; set; }
        public bool Error { get; set; }
        public bool IsProperty { get; set; }
        public string ReturnType { get; set; }
        public string Hash => GetMethodHash(Name, Parameters);
        public MethodParameterInfo[] Parameters { get; set; }

        public SourceCodeInfo SourceCode { get; set; }


        public override string ToString()
        {
            return $"{Name}";
        }

        private static string GetMethodHash(string methodName, MethodParameterInfo[] parameterInfos)
        {
            return $"{methodName}_{SerializeParametersInfo(parameterInfos)}";
        }

        private static string SerializeParametersInfo(MethodParameterInfo[] parameters)
        {
            return
                parameters == null
                    ? null
                    : $"{string.Join(";", parameters.Select(x => $"{x.Type},{x.Name},{string.Join("|", x.Modifiers)}"))}";
        }
    }
}