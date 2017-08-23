# README #

This README would normally document whatever steps are necessary to get your application up and running.

### What is this repository for? ###

TollEnnobler can help you to modifiy your c# projects with Microsoft.Analysis

### How do I get set up? ###

To run the TollEnnobler write a simple peace of code

```cs
var fixerRunner = new FixerRunner();

fixerRunner.Run(new Settings
	{
		Log4NetFileName = "log4net.config", // path to log4net config file
		ProjectNameFilter = x => x.Contains("BadProjects"), // filter solution projects for analysis
		RootNamespaceForNinjectConfiguring = "MyNamespace", // Namespace prefix for autoconfiguring DI by Ninject
		SolutionPath = "C:/_work/Mega.sln" // Path to solution for analysis
	});
```
	
Then you have to create IFixer implementations.
Changes to solution will be applied after every IFixer and will be flush after all IFixer-s

IFixer example. This fixer removes the "OneTimeSetUp" attribute from all classes in solution
```cs

    public class DummyFixer : IFixer
    {
        public string Name => "Dummy";
        public int Order => 42;
        public void Fix(Document document, DocumentEditor documentEditor)
        {
            foreach (var methodDeclaration in document.GetMethodDeclarations())
            {
                documentEditor.RemoveAttributeListFromMethod(methodDeclaration, "OneTimeSetUp");
            }
        }
    }
```

Install nuget package https://www.nuget.org/packages/Tolltech.Ennobler/
Or run command in nyget package manager Install-Package Tolltech.Ennobler
