using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Tolltech.Ennobler;
using Tolltech.Ennobler.SolutionFixers;
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

            public Task FixAsync(Document document, DocumentEditor documentEditor)
            {
                documents.Add(document);

                return Task.CompletedTask;
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
        public async Task TestGetDocuments(string frameworkVersion)
        {
            var documents = new List<Document>();
            var success = await fixerRunner.RunAsync(new Settings
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
