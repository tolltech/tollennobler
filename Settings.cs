using System;

namespace Tolltech.TollEnnobler
{
    public class Settings : ISettings
    {
        public string RootNamespaceForNinjectConfiguring { get; set; }
        public string Log4NetFileName { get; set; }
        public Func<string, bool> ProjectNameFilter { get; set; }
        public string SolutionPath { get; set; }
        public string VisualStudioInstallationPath { get; set; }
        public string VisualStudioVersion { get; set; }
    }
}