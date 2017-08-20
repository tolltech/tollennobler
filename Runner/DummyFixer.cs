using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Tolltech.TollEnnobler.Helpers;
using Tolltech.TollEnnobler.SolutionFixers;

namespace Tolltech.Runner
{
    public class DummyFixer : IFixer
    {
        public string Name => "BlaDummy";
        public int Order => 42;
        public void Fix(Document document, DocumentEditor documentEditor)
        {
            foreach (var methodDeclaration in document.GetMethodDeclarations())
            {
                documentEditor.RemoveAttributeListFromMethod(methodDeclaration ,"OneTimeSetUp");
            }         
        }
    }

    public class DummyFixer2 : IFixer
    {
        public string Name => "BlaDummy2";
        public int Order => 43;
        public void Fix(Document document, DocumentEditor documentEditor)
        {
            foreach (var classDeclaration in document.GetClassMethodDeclarations().Select(x => x.ClassDeclaration).Distinct())
            {
                var attrList = SyntaxFactoryExtensions.CreateAttributeList("TestFixture");

                //documentEditor.AddUsingIfDoesntExists("FluentAssertions");

                //documentEditor.AddAttributeListToClass(classDeclaration, attrList);
            }
        }
    }
}