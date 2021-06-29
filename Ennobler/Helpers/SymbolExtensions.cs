using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Tolltech.Ennobler.Models;

namespace Tolltech.Ennobler.Helpers
{
    public static class SymbolExtensions
    {
        public static ITypeSymbol[] GetBaseClasses(this ITypeSymbol symbol)
        {
            var result = new List<ITypeSymbol>();

            var s = symbol.BaseType;
            while (true)
            {
                if (s == null)
                {
                    return result.ToArray();
                }

                result.Add(s);
                s = s.BaseType;
            }
        }

        public static FullMethodName GetFullMethodName(this ITypeSymbol symbol, string methodName)
        {
            return new FullMethodName
            {
                MethodName = methodName,
                NamespaceName = symbol.ContainingNamespace.ToDisplayString(),
                ClassName = symbol.Name
            };
        }
    }
}