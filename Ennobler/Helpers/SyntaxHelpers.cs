using Microsoft.CodeAnalysis;

namespace Tolltech.Ennobler.Helpers
{
    public static class SyntaxHelpers
    {
        public static T FindParent<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            var s = syntaxNode.Parent;

            while (true)
            {
                if (s == null)
                {
                    return null;
                }

                if (s is T result)
                {
                    return result;
                }

                s = s.Parent;
            }
        }

        public static bool ParametersAreSuitableSimple(string[] simpleTypesToFound, string[] complexTypes)
        {
            if (simpleTypesToFound.Length > complexTypes.Length)
            {
                return false;
            }

            for (var i = 0; i < simpleTypesToFound.Length; ++i)
            {
                var simpleType = simpleTypesToFound[i];
                var complexType = complexTypes[i];

                if (complexType.Contains("<"))
                {
                    continue;
                }

                if (!complexType.Contains(simpleType.Trim()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}