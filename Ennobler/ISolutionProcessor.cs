﻿using System.Threading.Tasks;
using Tolltech.Ennobler.SolutionFixers;
using Tolltech.Ennobler.SolutionGraph;

namespace Tolltech.Ennobler
{
    public interface ISolutionProcessor
    {
        Task<bool> ProcessAsync(string solutionPath, IFixer[] fixers);
        Task ProcessAsync(string solutionPath, IAnalyzer[] analyzers);
    }
}