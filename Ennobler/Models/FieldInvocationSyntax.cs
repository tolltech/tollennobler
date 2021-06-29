using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tolltech.Ennobler.Models
{
    public class FieldInvocationSyntax
    {
        public string MethodName { get; set; }
        public InvocationExpressionSyntax FieldMethodInvocationExpression { get; set; }
    }
}