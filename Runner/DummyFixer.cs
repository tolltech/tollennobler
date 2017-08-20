using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Tolltech.TollEnnobler.SolutionFixers;

namespace Tolltech.Runner
{
    public class DummyFixer : IFixer
    {
        public string Name => "BlaDummy";
        public int Order => 42;
        public void Fix(Document document, DocumentEditor documentEditor)
        {
            Console.WriteLine(":-)");
        }
    }
}