using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tolltech.Ennobler.Helpers
{
    public static class MethodDeclarationSyntaxExtensions
    {
        public static MethodDeclarationSyntax RemoveAttribute(this MethodDeclarationSyntax methodDeclaration, string attributeName)
        {
            var attributeListWithExpectedExcetion = methodDeclaration.AttributeLists
                .FirstOrDefault(x =>
                    x.Attributes
                        .Any(y => (y.Name as IdentifierNameSyntax)
                                  ?.Identifier.ValueText == attributeName));

            var expectedExceptionAttribute = attributeListWithExpectedExcetion?.Attributes
                .FirstOrDefault(x =>
                    (x.Name as IdentifierNameSyntax)
                    ?.Identifier.ValueText == attributeName);

            if (expectedExceptionAttribute == null)
                return methodDeclaration;


            var newAttrsWithoutExcpectedException = attributeListWithExpectedExcetion.Attributes.Remove(expectedExceptionAttribute);

            SyntaxList<AttributeListSyntax> newAttrLists;
            if (newAttrsWithoutExcpectedException.Count == 0)
            {
                newAttrLists = methodDeclaration.AttributeLists.Remove(attributeListWithExpectedExcetion);
            }
            else
            {
                var newAttrListWithoutExcpextedExcpetion = attributeListWithExpectedExcetion.WithAttributes(newAttrsWithoutExcpectedException);
                newAttrLists = methodDeclaration.AttributeLists.Replace(attributeListWithExpectedExcetion, newAttrListWithoutExcpextedExcpetion);
            }
            

            return methodDeclaration.WithAttributeLists(newAttrLists);
        }
    }
}