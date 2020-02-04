namespace Tolltech.Ennobler.SolutionFixers
{
    public interface ISolutionProcessor
    {
        bool Process(string solutionPath, IFixer[] fixers);
    }
}