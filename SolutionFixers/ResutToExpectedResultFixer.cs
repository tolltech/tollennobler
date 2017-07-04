using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.TollEnnobler.SolutionFixers
{
    public class ResutToExpectedResultFixer : IFixer
    {
        public FixerGroup Group => FixerGroup.NUnitMigrator;
        public string Name => "ReslutToEcpetedResult";
        public int Order => 2;

        public void Fix(Document document, DocumentEditor documentEditor)
        {
            var documentSyntaxTree = document.GetSyntaxTreeAsync().Result;
            var classDeclarationSyntaxes =
                documentSyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var methodDesclarationSyntaxes =
                    classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
                var methodSyntaxNodes =
                    methodDesclarationSyntaxes.ToArray();

                foreach (var methodSyntaxNode in methodSyntaxNodes)
                {                    
                    var newAttrLists = methodSyntaxNode.AttributeLists;

                    while (true)
                    {
                        var attributeListsWithTestCase = newAttrLists
                            .FirstOrDefault(attrList =>
                                attrList.Attributes
                                    .Any(attr => (attr.Name as IdentifierNameSyntax)?.Identifier.ValueText == "TestCase"
                                                 && (attr.ArgumentList?.Arguments.Any(arg => arg.NameEquals?.Name?.Identifier.ValueText == "Result") ?? false)));

                        if (attributeListsWithTestCase == null)
                            break;

                        var testCaseAttribute = attributeListsWithTestCase.Attributes
                       .First(x =>
                           (x.Name as IdentifierNameSyntax)
                               ?.Identifier.ValueText == "TestCase");

                        var resultAttr = testCaseAttribute.ArgumentList.Arguments.First(x => x.NameEquals?.Name?.Identifier.ValueText == "Result");

                        var newArguments = testCaseAttribute.ArgumentList.Arguments.Replace(resultAttr,
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("ExpectedResult")), null, resultAttr.Expression).NormalizeWhitespace());
                        var newArgumentList = testCaseAttribute.ArgumentList.WithArguments(newArguments);
                        var newTestCaseAttribute = testCaseAttribute.WithArgumentList(newArgumentList);
                        var newAttrsWithoutExcpectedException = attributeListsWithTestCase.Attributes.Replace(testCaseAttribute, newTestCaseAttribute);
                        var newAttrListWithoutExcpextedExcpetion = attributeListsWithTestCase.WithAttributes(newAttrsWithoutExcpectedException);
                        newAttrLists = newAttrLists.Replace(attributeListsWithTestCase, newAttrListWithoutExcpextedExcpetion);
                    }

                    if (methodSyntaxNode.AttributeLists != newAttrLists)
                        documentEditor.ReplaceNode(methodSyntaxNode, methodSyntaxNode.WithAttributeLists(newAttrLists));
                }
            }
        }
    }
}