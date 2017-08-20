using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.TollEnnobler.Helpers
{
    public static class DocumentEditorExtensions
    {
        public static void AddAttributeListToClass(this DocumentEditor documentEditor, ClassDeclarationSyntax classDeclaration, AttributeListSyntax attributeList)
        {
            documentEditor.ReplaceNode(classDeclaration, classDeclaration.AddAttributeLists(attributeList));
        }

        public static void RemoveAttributeListFromMethod(this DocumentEditor documentEditor, MethodDeclarationSyntax methodDeclaration, string attributeName)
        {
            documentEditor.ReplaceNode(methodDeclaration, methodDeclaration.RemoveAttribute(attributeName));
        }

    }
}