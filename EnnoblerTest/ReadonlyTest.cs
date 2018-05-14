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
            private readonly List<Document> documents;
            public string Name => "TestFixer";
            public int Order => 1;

            public TestFixer(List<Document> documents)
            {
                this.documents = documents;
            }

            public void Fix(Document document, DocumentEditor documentEditor)
            {
                documents.Add(document);
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
            var documents = new List<Document>();
            var success = fixerRunner.Run(new Settings
            {
                Log4NetFileName = null,
                ProjectNameFilter = x => x == $"Test{frameworkVersion}Project",
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = $"../../../../Tests/TestSolution{frameworkVersion}.sln",
            }, new IFixer[] { new TestFixer(documents) });

            Assert.True(success);

            Assert.NotEmpty(documents);

            Assert.Contains(documents, x => x.Name == "ClassForReadonly.cs");
        }
    }
}
