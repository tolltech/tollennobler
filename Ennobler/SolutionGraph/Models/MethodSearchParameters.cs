using System;
using Microsoft.CodeAnalysis;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class MethodSearchParameters
    {
        public MethodSearchParameters()
        {
            Parameters = Array.Empty<ITypeSymbol>();
            Classes = Array.Empty<ITypeSymbol>();
        }

        public string MethodName { get; set; }
        public ITypeSymbol[] Parameters { get; set; }
        public ITypeSymbol[] Classes { get; set; }
    }
}