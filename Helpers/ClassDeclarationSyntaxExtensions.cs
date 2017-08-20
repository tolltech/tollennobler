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
    }
}