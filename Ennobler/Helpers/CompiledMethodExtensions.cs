using System.Linq;
using Microsoft.CodeAnalysis;
using Tolltech.Ennobler.SolutionGraph.Helpers;
using Tolltech.Ennobler.SolutionGraph.Models;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler.Helpers
{
    public static class CompiledMethodExtensions
    {
        private static readonly ILog log = LogProvider.Get().ForContext(typeof(CompiledMethodExtensions));

        public static bool ParametersAreSuitable(this CompiledMethod sourceMethod, CompiledMethod targetMethod, out bool? identity)
        {
            var targetMethodParameters = targetMethod.Parameters.Select(x => targetMethod.SemanticModel.GetSymbolInfo(x.Type).Symbol as ITypeSymbol).ToArray();

            return ParametersAreSuitable(sourceMethod, targetMethodParameters, out identity);
        }

        public static bool ParametersAreSuitable(this CompiledMethod sourceMethod, ITypeSymbol[] targetMethodParameters, out bool? identity)
        {
            identity = null;
            var sourceMethodParameters = sourceMethod.Parameters;
            var strongSourceParameters = sourceMethodParameters
                .TakeWhile(x => x.Default == null && x.Modifiers.All(y => y.ValueText != "params")).ToArray();

            if (targetMethodParameters.Length < strongSourceParameters.Length)
            {
                return false;
            }

            for (var i = 0; i < targetMethodParameters.Length; ++i)
            {
                if (i >= sourceMethodParameters.Length)
                {
                    return false;
                }

                var targetMethodParameter = targetMethodParameters[i];
                var declaredParameter = sourceMethodParameters[i];

                var declaredTypeSymbol = sourceMethod.SemanticModel.GetSymbolInfo(declaredParameter.Type).Symbol as ITypeSymbol;

                if (declaredTypeSymbol == null)
                {
                    return false;
                }

                if (targetMethodParameter == null)
                {
                    if (declaredTypeSymbol.IsValueType && declaredTypeSymbol.Name != "Nullable")
                    {
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (targetMethodParameter.ToString() == declaredTypeSymbol.ToString())
                {
                    continue;
                }

                var conversion = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.ClassifyConversion(sourceMethod.Compilation,
                    targetMethodParameter, declaredTypeSymbol);
                if (conversion.Exists && conversion.IsImplicit)
                {
                    identity = false;
                    continue;
                }

                if (declaredParameter.Modifiers.Any(x => x.Text == "params"))
                {
                    var declaredParameterElementType = (declaredTypeSymbol as IArrayTypeSymbol)?.ElementType;
                    if (declaredParameterElementType == null)
                    {
                        log.Error(
                            $"Strange params parameterType of {sourceMethod.Name} in document {sourceMethod.DocumentSyntaxTree.FilePath} {declaredTypeSymbol}");
                        $"Strange params parameterType,{sourceMethod.Name},{sourceMethod.DocumentSyntaxTree.FilePath},{declaredTypeSymbol}"
                            .ToStructuredLogFile();
                        return false;
                    }

                    if (targetMethodParameter.ToString() != declaredParameterElementType.ToString())
                    {
                        continue;
                    }

                    conversion = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.ClassifyConversion(sourceMethod.Compilation,
                        targetMethodParameter, declaredParameterElementType);
                    if (!(conversion.Exists && conversion.IsImplicit))
                    {
                        return false;
                    }

                    identity = false;
                    continue;
                }
                else
                {
                    return false;
                }
            }

            if (!identity.HasValue)
            {
                identity = true;
            }

            return true;
        }
    }
}