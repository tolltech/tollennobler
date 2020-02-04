using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tolltech.Ennobler.Helpers
{
    public static class SyntaxFactoryExtensions
    {
        public static AttributeListSyntax CreateAttributeList(string attributeName, string enumParamTypeName, string enumValueName)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName))
                    .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(enumParamTypeName),
                                SyntaxFactory.IdentifierName(enumValueName)
                            ))))
                    ))).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.Whitespace("\r\n")).WithLeadingTrivia(SyntaxFactory.Whitespace("    "));
        }

        public static AttributeListSyntax CreateAttributeList(string attributeName)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName))
                )).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.Whitespace("\r\n"))
                .WithLeadingTrivia(SyntaxFactory.Whitespace("    "));
        }
    }
}