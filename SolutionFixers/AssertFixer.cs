using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.TollEnnobler.SolutionFixers
{
    public class AssertFixer : IFixer
    {
        public FixerGroup Group => FixerGroup.NUnitMigrator;
        public string Name => "AssertFixer";
        public int Order => 5;

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
                    var attributetestCase = methodSyntaxNode.AttributeLists
                        .FirstOrDefault(x =>
                            x.Attributes
                                .Any(y => (y.Name as IdentifierNameSyntax)
                                    ?.Identifier.ValueText.Contains("Test") ?? false));

                    if (attributetestCase == null)
                        continue;

                    var expressionStatements = methodSyntaxNode.Body.Statements.ToArray();
                    foreach (var statement in expressionStatements)
                    {
                        StatementSyntax newStatement;

                        var expressionStatement = statement as ExpressionStatementSyntax;
                        if (expressionStatement != null)
                        {
                            newStatement = GetNewAssertStatement(expressionStatement);
                        }
                        else
                        {
                            newStatement = GetNewStatement<UsingStatementSyntax>(statement, x => x.Statement as BlockSyntax);
                            if (newStatement == statement)
                                newStatement = GetNewStatement<ForStatementSyntax>(statement, x => x.Statement as BlockSyntax);
                            if (newStatement == statement)
                                newStatement = GetNewStatement<ForEachStatementSyntax>(statement, x => x.Statement as BlockSyntax);
                            if (newStatement == statement)
                                newStatement = GetNewStatement<WhileStatementSyntax>(statement, x => x.Statement as BlockSyntax);
                        }

                        if (statement != newStatement)
                        {
                            documentEditor.AddUsingIfNeed("FluentAssertions");
                            documentEditor.ReplaceNode(statement, newStatement);
                        }
                    }
                }
            }
        }

        private StatementSyntax GetNewStatement<T>(StatementSyntax blockStatement, Func<T, BlockSyntax> getBlock) where T : StatementSyntax
        {
            var typedBlockStatement = blockStatement as T;
            if (typedBlockStatement == null)
                return blockStatement;

            var oldBlock = getBlock(typedBlockStatement);
            var newBlock = oldBlock;
            if (newBlock == null)
                return blockStatement;

            while (true)
            {
                var statmenets = newBlock.Statements.OfType<ExpressionStatementSyntax>().ToArray();
                var wasChanged = false;
                foreach (var statmenet in statmenets)
                {
                    var newStatment = GetNewAssertStatement(statmenet);
                    if (newStatment != statmenet)
                    {
                        newBlock = newBlock.ReplaceNode(statmenet, newStatment);
                        wasChanged = true;
                        break;
                    }
                }

                if (!wasChanged)
                    break;
            }

            if (newBlock == oldBlock)
                return blockStatement;

            return blockStatement.ReplaceNode(oldBlock, newBlock);
        }

        private ExpressionStatementSyntax GetNewAssertStatement(ExpressionStatementSyntax expressionStatement)
        {
            var invocationExpression = expressionStatement.Expression as InvocationExpressionSyntax;
            var memberAccessExpression =
                invocationExpression?.Expression as MemberAccessExpressionSyntax;
            var identifierNameExpression = memberAccessExpression?.Expression as IdentifierNameSyntax;

            var identifierValueText = identifierNameExpression?.Identifier.ValueText;
            if (identifierValueText == null || identifierValueText != "Assert")
                return expressionStatement;

            var assertMethodName = memberAccessExpression.Name.Identifier.ValueText;
            ExpressionStatementSyntax newStatment;
            switch (assertMethodName)
            {
                case "IsNotNullOrEmpty":
                    newStatment = GetNullOrNotStatement(invocationExpression, "NotBeNullOrEmpty");
                    break;
                case "IsNullOrEmpty":
                    newStatment = GetNullOrNotStatement(invocationExpression, "BeNullOrEmpty");
                    break;
                case "That":
                    newStatment = GetShouldBeEquilanetStatement(invocationExpression);
                    break;
                default:
                    return expressionStatement;
            }

            if (newStatment == null)
                return expressionStatement;

            return newStatment.WithTrailingTrivia(SyntaxFactory.Whitespace("\r\n")).WithLeadingTrivia(expressionStatement.GetLeadingTrivia());
        }

        private ExpressionStatementSyntax GetShouldBeEquilanetStatement(InvocationExpressionSyntax assertInvocationExpression)
        {
            if (assertInvocationExpression.ArgumentList.Arguments.Count != 2)
                return null;

            var actualArgument = assertInvocationExpression.ArgumentList.Arguments.First();
            var dataContractIsArgument = assertInvocationExpression.ArgumentList.Arguments.Last();

            var invocationExpression = dataContractIsArgument.Expression as InvocationExpressionSyntax;
            var memberAccessExpression = invocationExpression?.Expression as MemberAccessExpressionSyntax;
            var identifierNameExpression = memberAccessExpression?.Expression as IdentifierNameSyntax;

            var identifierValueText = identifierNameExpression?.Identifier.ValueText;
            var memberName = memberAccessExpression?.Name.Identifier.ValueText;
            if (identifierValueText == null || identifierValueText != "DataContractIs" || (memberName != "EqualTo" && memberName != "EquivalentTo"))
                return null;

            if (invocationExpression.ArgumentList.Arguments.Count != 1)
                return null;

            var newExpression = actualArgument.Expression is CastExpressionSyntax
                ? SyntaxFactory.ParenthesizedExpression(actualArgument.Expression)
                : actualArgument.Expression;

            var shouldInvoxcationExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, newExpression
                    ,
                    SyntaxFactory.IdentifierName("ShouldBeEquivalentTo"))
            );

            return
                SyntaxFactory.ExpressionStatement(
                    shouldInvoxcationExpression.WithArgumentList(invocationExpression.ArgumentList)
                );
        }

        private ExpressionStatementSyntax GetNullOrNotStatement(InvocationExpressionSyntax invocationExpression, string fluentAssertionsMethod)
        {
            if (invocationExpression.ArgumentList.Arguments.Count != 1)
                return null;

            var argument = invocationExpression.ArgumentList.Arguments.Single();

            var shouldInvoxcationExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, argument.Expression,
                    SyntaxFactory.IdentifierName("Should"))
            );

            return
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, shouldInvoxcationExpression,
                            SyntaxFactory.IdentifierName(fluentAssertionsMethod))
                    )
                );
        }
    }
}