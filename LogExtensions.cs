using System;
using log4net;

namespace Tolltech.TollEnnobler
{
    public static class LogExtensions
    {
        public static void ToConsole(this ILog log, string msg)
        {
            log?.Info(msg);
            Console.WriteLine(msg);
        }

        public static void ToError(this ILog log, string msg)
        {
            log?.Error(msg);
            Console.Error.WriteLine(msg);
        }
    }
}