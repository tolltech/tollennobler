using System;

namespace Tolltech.Ennobler
{
    public class Settings : ISettings
    {
        public string RootNamespaceForNinjectConfiguring { get; set; }
        public string Log4NetFileName { get; set; }
        public Func<string, bool> ProjectNameFilter { get; set; }
        public string SolutionPath { get; set; }
    }
}