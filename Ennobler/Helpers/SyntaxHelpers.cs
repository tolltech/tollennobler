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

    }
}