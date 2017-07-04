using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.TollEnnobler.SolutionFixers
{
    public class ClearTableToNonParallizeFixerFixer : IFixer
    {
        public FixerGroup Group => FixerGroup.ParallelizeTest;
        public string Name => "DbTruncateFix";
        public int Order => 0;

        private static readonly HashSet<string> badMethods = new HashSet<string>
        {
            "ClearTable", "SelectAll", "Count", "DeleteAll", "ResetCounters", "Find", "InsertAll",
            "TruncateTable", "TruncateColumnFamily", "DeleteRows", "TruncateKeyspace", "ExecuteCommand", "ExecuteCommand", "TruncateElasticIndex", "TruncateAll", "DeleteMapping", "DeleteIndex"
        };

        public void Fix(Document document, DocumentEditor documentEditor)
        {
            var documentSyntaxTree = document.GetSyntaxTreeAsync().Result;
            var classDeclarationSyntaxes =
                documentSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            var nonParallelAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Parallelizable"))
                    .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("ParallelScope"),
                                SyntaxFactory.IdentifierName("None")
                            ))))
                    ))).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.Whitespace("\r\n")).WithLeadingTrivia(SyntaxFactory.Whitespace("    "));

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var methodDesclarationSyntaxes =
                    classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
                var methodSyntaxNodes =
                    methodDesclarationSyntaxes.ToArray();

                var nonParallelAtrributeExists = classDeclarationSyntax.AttributeLists.Any(x=>x.Attributes.Any(y=>y.Name.ToString() == "Parallelizable"));
                if (nonParallelAtrributeExists)
                    continue;

                foreach (var methodSyntaxNode in methodSyntaxNodes)
                {
                    //if (methodSyntaxNode.Identifier.ValueText != "SetUp")
                    //    continue;

                    var setupMethods = methodSyntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();
                    var clearTableExists = setupMethods.Any(x => badMethods.Contains((x.Expression as GenericNameSyntax)?.Identifier.ValueText))
                        || setupMethods.Any(x => badMethods.Contains((x.Expression as MemberAccessExpressionSyntax)?.Name.Identifier.ValueText));

                    if (!clearTableExists)
                        continue;

                    var newAttrLists = classDeclarationSyntax.AttributeLists;

                    newAttrLists = newAttrLists.Add(nonParallelAttribute);

                    documentEditor.ReplaceNode(classDeclarationSyntax, classDeclarationSyntax.WithAttributeLists(newAttrLists));
                    break;

                    //var newAttrLists = methodSyntaxNode.AttributeLists;

                    //while (true)
                    //{
                    //    var attributeListsWithTestCase = newAttrLists
                    //        .FirstOrDefault(attrList =>
                    //            attrList.Attributes
                    //                .Any(attr => (attr.Name as IdentifierNameSyntax)?.Identifier.ValueText == "TestCase"
                    //                             && (attr.ArgumentList?.Arguments.Any(arg => arg.NameEquals?.Name?.Identifier.ValueText == "Result") ?? false)));

                    //    if (attributeListsWithTestCase == null)
                    //        break;

                    //    var testCaseAttribute = attributeListsWithTestCase.Attributes
                    //        .First(x =>
                    //            (x.Name as IdentifierNameSyntax)
                    //            ?.Identifier.ValueText == "TestCase");

                    //    var resultAttr = testCaseAttribute.ArgumentList.Arguments.First(x => x.NameEquals?.Name?.Identifier.ValueText == "Result");

                    //    var newArguments = testCaseAttribute.ArgumentList.Arguments.Replace(resultAttr,
                    //        SyntaxFactory.AttributeArgument(
                    //            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("ExpectedResult")), null, resultAttr.Expression).NormalizeWhitespace());
                    //    var newArgumentList = testCaseAttribute.ArgumentList.WithArguments(newArguments);
                    //    var newTestCaseAttribute = testCaseAttribute.WithArgumentList(newArgumentList);
                    //    var newAttrsWithoutExcpectedException = attributeListsWithTestCase.Attributes.Replace(testCaseAttribute, newTestCaseAttribute);
                    //    var newAttrListWithoutExcpextedExcpetion = attributeListsWithTestCase.WithAttributes(newAttrsWithoutExcpectedException);
                    //    newAttrLists = newAttrLists.Replace(attributeListsWithTestCase, newAttrListWithoutExcpextedExcpetion);
                    //}

                    //if (methodSyntaxNode.AttributeLists != newAttrLists)
                    //    documentEditor.ReplaceNode(methodSyntaxNode, methodSyntaxNode.WithAttributeLists(newAttrLists));
                }
            }
        }
    }
}