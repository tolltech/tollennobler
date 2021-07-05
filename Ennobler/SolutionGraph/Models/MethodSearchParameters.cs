using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class MethodSearchParameters
    {
        public MethodSearchParameters([NotNull] string methodName, [ItemNotNull] [NotNull] ITypeSymbol[] classes, [ItemCanBeNull] [NotNull] ITypeSymbol[] parameters)
        {
            MethodName = methodName;
            Parameters = parameters;
            Classes = classes;
        }

        [NotNull] public string MethodName { get; }
        [ItemCanBeNull] [NotNull] public ITypeSymbol[] Parameters { get; }
        [ItemNotNull] [NotNull] public ITypeSymbol[] Classes { get; }
    }
}