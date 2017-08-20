# README #

This README would normally document whatever steps are necessary to get your application up and running.

### What is this repository for? ###

TollEnnobler can help you to modifiy your c# projects with Microsoft.Analys

### How do I get set up? ###

To run the TollEnnobler write a simple peace of code

var fixerRunner = new FixerRunner();

fixerRunner.Run(new Settings
	{
		Log4NetFileName = "log4net.config",
		ProjectNameFilter = x => x.Contains("BadProjects"),
		RootNamespaceForNinjectConfiguring = "MyNamespace",
		SolutionPath = "C:/_work/Mega.sln"
	});
	
Then you have to create IFixer implementations.
Changes to solution will be applied after every IFixer and will be flush after all IFixer-s