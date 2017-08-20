﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Tolltech.TollEnnobler.Helpers
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
            var newAttrListWithoutExcpextedExcpetion = attributeListWithExpectedExcetion.WithAttributes(newAttrsWithoutExcpectedException);
            var newAttrLists = methodDeclaration.AttributeLists.Replace(attributeListWithExpectedExcetion, newAttrListWithoutExcpextedExcpetion);

            return methodDeclaration.WithAttributeLists(newAttrLists);
        }
    }
}