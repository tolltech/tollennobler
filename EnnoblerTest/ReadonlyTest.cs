using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Tolltech.Ennobler;
using Tolltech.Ennobler.SolutionFixers;
using Tolltech.Ennobler.SolutionGraph;
using Tolltech.Ennobler.SolutionGraph.Models;
using Xunit;

namespace Tolltech.EnnoblerTest
{
    public class ReadOnlyTest
    {
        private Runner _runner;

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

        public class TestAnalyzer : IAnalyzer
        {
            private CompiledSolution compiledSolution;

            public Task AnalyzeAsync(CompiledSolution compiledSolution)
            {
                this.compiledSolution = compiledSolution;
                return Task.CompletedTask;
            }

            public CompiledMethod[] GetMethods(string namespaceName, string className, string methodName)
            {
                return compiledSolution.CompiledProjects.GetMethods(namespaceName, className, methodName);
            }
        }


        public ReadOnlyTest()
        {
            _runner = new Runner();
        }

        [Theory]
        [InlineData("Core")]
        [InlineData("Framework")]
        [InlineData("Standard")]
        public async Task TestGetDocuments(string frameworkVersion)
        {
            var documents = new List<Document>();
            var success = await _runner.RunFixersAsync(new Settings
            {
                Log4NetFileName = null,
                ProjectNameFilter = x => x == $"Test{frameworkVersion}Project",
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = $"../../../../Tests/TestSolution{frameworkVersion}.sln",
            }, new IFixer[] { new TestFixer(documents) }).ConfigureAwait(false);

            Assert.True(success);

            Assert.NotEmpty(documents);

            Assert.Contains(documents, x => x.Name == "ClassForReadonly.cs");
        }

        [Theory]
        [InlineData("Core", "TestCoreProject", "TestCoreClass", "TestMethod", true)]
        [InlineData("Core", "TestCoreProject", "TestCoreClass", "TestMethod2", false)]
        [InlineData("Framework", "TestFrameworkProject", "TestClass", "TestMethod", true)]
        [InlineData("Framework", "TestFrameworkProject", "TestClass", "TestMethod2", false)]
        [InlineData("Standard", "TestStandardProject", "StandardClass", "TestMethod", true)]
        [InlineData("Standard", "TestStandardProject", "StandardClass", "TestMethod2", false)]
        public async Task TestAnalyze(string frameworkVersion, string namespaceName, string className, string methodName, bool expected)
        {
            var testAnalyzer = new TestAnalyzer();
            await _runner.RunAnalyzersAsync(new Settings
            {
                Log4NetFileName = null,
                ProjectNameFilter = x => x == $"Test{frameworkVersion}Project",
                RootNamespaceForNinjectConfiguring = "Tolltech",
                SolutionPath = $"../../../../Tests/TestSolution{frameworkVersion}.sln",
            }, new IAnalyzer[] {testAnalyzer}).ConfigureAwait(false);

            var methods = testAnalyzer.GetMethods(namespaceName, className, methodName);
            Assert.Equal(methods.Length > 0, expected);
        }

    }
}
