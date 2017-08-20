using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tolltech.TollEnnobler.Helpers
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static ClassDeclarationSyntax AddAttribute(this ClassDeclarationSyntax classDeclaration, AttributeListSyntax attributeList)
        {
            var newAttrLists = classDeclaration.AttributeLists;
            newAttrLists = newAttrLists.Add(attributeList);
            return classDeclaration.WithAttributeLists(newAttrLists);
        }

        public static IEnumerable<ClassDeclarationSyntax> GetParentClassDeclarations(this ClassDeclarationSyntax classDeclaration)
        {
            var parent = classDeclaration.Parent;
            while (parent != null)
            {
                var parentClassDeclaration = parent as ClassDeclarationSyntax;

                if (parentClassDeclaration != null)
                {
                    yield return parentClassDeclaration;
                }

                parent = parent.Parent;
            }
        }
    }
}