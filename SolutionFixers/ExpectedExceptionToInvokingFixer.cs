using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.TollEnnobler.SolutionFixers
{
    public class ExpectedExceptionToInvokingFixer : IFixer
    {
        public FixerGroup Group => FixerGroup.NUnitMigrator;
        public string Name => "ExpectedException";
        public int Order => 0;

        public void Fix(Document document, DocumentEditor documentEditor)
        {
            var documentSyntaxTree = document.GetSyntaxTreeAsync().Result;
            var classDeclarationSyntaxes = documentSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var methodDesclarationSyntaxes =
                    classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
                var methodSyntaxNodes =
                    methodDesclarationSyntaxes.ToArray();

                foreach (var methodSyntaxNode in methodSyntaxNodes)
                {
                    var attributeListWithExpectedExcetion = methodSyntaxNode.AttributeLists
                        .FirstOrDefault(x =>
                            x.Attributes
                                .Any(y => (y.Name as IdentifierNameSyntax)
                                              ?.Identifier.ValueText == "ExpectedException"));

                    var expectedExceptionAttribute = attributeListWithExpectedExcetion?.Attributes
                        .FirstOrDefault(x =>
                            (x.Name as IdentifierNameSyntax)
                                ?.Identifier.ValueText == "ExpectedException");

                    if (expectedExceptionAttribute == null)
                        continue;

                    var exceptionTypeName = expectedExceptionAttribute.ArgumentList.Arguments
                        .OfType<AttributeArgumentSyntax>()
                        .Select(x => x.Expression)
                        .OfType<TypeOfExpressionSyntax>()
                        .Select(x => x.Type)
                        .OfType<IdentifierNameSyntax>()
                        .FirstOrDefault()?.Identifier.ValueText;

                    var lastStatement = methodSyntaxNode.Body.Statements.LastOrDefault() as ExpressionStatementSyntax;
                    var invocationExpression = lastStatement?.Expression as InvocationExpressionSyntax;
                    var memberAccessExpression = invocationExpression?.Expression as MemberAccessExpressionSyntax;
                    if (memberAccessExpression == null)
                        continue;

                    var memberOwnerExpression = memberAccessExpression.Expression;
                    var memberName = (memberAccessExpression.Name as IdentifierNameSyntax)?.Identifier.ValueText;
                    if (memberName == null)
                        continue;

                    documentEditor.AddUsingIfNeed("FluentAssertions");
                    WrapLastStatementByShouldThrow(memberOwnerExpression, memberName, invocationExpression, exceptionTypeName,
                        documentEditor, lastStatement);
                }
            }
        }

        private static void WrapLastStatementByShouldThrow(ExpressionSyntax memberOwnerExpression, string memberName,
            InvocationExpressionSyntax invocationExpression, string exceptionTypeName, DocumentEditor documentEditor,
            ExpressionStatementSyntax lastStatement)
        {
            var invoking = SyntaxFactory.IdentifierName("Invoking");
            var invokingMemberExpression =
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    memberOwnerExpression, invoking);

            var invokingLambda = SyntaxFactory.SimpleLambdaExpression(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("x")
                ),
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("x"),
                        SyntaxFactory.IdentifierName(memberName)
                    ),
                    invocationExpression.ArgumentList)
            );

            var argument = SyntaxFactory.Argument(invokingLambda);
            var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

            var invokingCall =
                SyntaxFactory.InvocationExpression(invokingMemberExpression,
                    SyntaxFactory.ArgumentList(argumentList));

            var shouldTrowIdentifier = SyntaxFactory.IdentifierName($"ShouldThrow<{exceptionTypeName}>");
            var shouldThrowCall =
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, invokingCall,
                            shouldTrowIdentifier)
                    )
                );

            documentEditor.ReplaceNode(lastStatement, shouldThrowCall.WithTrailingTrivia(SyntaxFactory.Whitespace("\r\n")));
        }
    }
}