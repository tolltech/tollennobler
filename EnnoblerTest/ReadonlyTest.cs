using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Tolltech.TollEnnobler;
using Tolltech.TollEnnobler.SolutionFixers;
using Xunit;

namespace Tolltech.EnnoblerTest
{
    public class ReadOnlyTest
    {
        private FixerRunner fixerRunner;

        public class TestFixer : IFixer
        {
            public string Name => "TestFixer";
            public int Order => 1;

            public static List<Document> Documents = new List<Document>();

            public void Fix(Document document, DocumentEditor documentEditor)
            {
                Documents.Add(document);
            }
        }

        public ReadOnlyTest()
        {
            fixerRunner = new FixerRunner();
        }

        [Theory]
        [InlineData("Core")]
        [InlineData("Framework")]
        [InlineData("Standard")]
        public void TestGetDocuments(string frameworkVersion)
        {
            var success = fixerRunner.Run(new Settings
            {
                Log4NetFileName = null,
                ProjectNameFilter = x => x == $"Test{frameworkVersion}Project",
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = $"../../../../Tests/TestSolution{frameworkVersion}.sln",
            }, new IFixer[] { new TestFixer() });

            Assert.True(success);

            var documents = TestFixer.Documents;
            Assert.NotEmpty(documents);

            Assert.Contains(documents, x => x.Name == "ClassForReadonly.cs");
        }
    }
}
