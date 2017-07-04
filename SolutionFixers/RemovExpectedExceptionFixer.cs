using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.TollEnnobler.SolutionFixers
{
    public class RemovExpectedExceptionFixer : IFixer
    {
        public FixerGroup Group => FixerGroup.NUnitMigrator;
        public string Name => "RemoveExpectedEception";
        public int Order => 10;

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


                    var newAttrsWithoutExcpectedException = attributeListWithExpectedExcetion.Attributes.Remove(expectedExceptionAttribute);
                    var newAttrListWithoutExcpextedExcpetion = attributeListWithExpectedExcetion.WithAttributes(newAttrsWithoutExcpectedException);
                    var newAttrLists = methodSyntaxNode.AttributeLists.Replace(attributeListWithExpectedExcetion, newAttrListWithoutExcpextedExcpetion);

                    documentEditor.AddUsingIfNeed("FluentAssertions");
                    documentEditor.ReplaceNode(methodSyntaxNode, methodSyntaxNode.WithAttributeLists(newAttrLists));
                }
            }
        }
    }
}