using System;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler.SolutionGraph.Helpers
{
    public static class LogExtensions
    {
        public static void ToConsole(this ILog log, string line)
        {
            log.Info(line);
            Console.WriteLine(line);
        }
    }
}