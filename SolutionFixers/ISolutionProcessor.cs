namespace Tolltech.TollEnnobler.SolutionFixers
{
    public interface ISolutionProcessor
    {
        bool Process(string solutionPath, IFixer[] fixers);
    }
}