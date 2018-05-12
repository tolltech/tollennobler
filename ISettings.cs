using System;

namespace Tolltech.TollEnnobler
{
    public interface ISettings
    {
        string RootNamespaceForNinjectConfiguring { get; }
        string Log4NetFileName { get; }
        Func<string, bool> ProjectNameFilter { get; }
        string SolutionPath { get; }
    }
}