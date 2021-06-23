using System;
using System.IO;
using System.Linq;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler.SolutionGraph.Helpers
{
    public static class StringExtensions
    {
        private static readonly ILog log = LogProvider.Get().ForContext(typeof(StringExtensions));

        public static string KillGenericTags(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            var openTagIndex = source.IndexOf("<");
            var closeTagIndex = source.LastIndexOf(">");

            if (((openTagIndex == -1 || closeTagIndex == -1) && openTagIndex != closeTagIndex)
                || openTagIndex > closeTagIndex)
            {
                log.Error($"Strange tag consistency in string {source}");
                $"Strange tag consistency in string,{source}".ToStructuredLogFile();
                return source;
            }

            return new string(source.Take(openTagIndex).Concat(source.Skip(closeTagIndex + 1)).ToArray());
        }

        public static string GetPostfix(this string source, string separator = ".")
        {
            return source.Substring(source.LastIndexOf(separator, StringComparison.InvariantCulture) + 1);
        }

        public static string GetPostfix(this string source, int skipSeparatorsCount, string separator = ".")
        {
            var startIndex = source.Length - 1;
            for (int i = 0; i < skipSeparatorsCount; i++)
            {
                if (startIndex < 0)
                {
                    return source;
                }

                startIndex = source.LastIndexOf(separator, startIndex, StringComparison.InvariantCulture) - 1;
            }

            return source.Substring(source.LastIndexOf(separator, startIndex, StringComparison.InvariantCulture) + 1);
        }

        public static void ToFile(this string line, string fileName)
        {
            using (var fileStream = File.AppendText(fileName))
            {
                fileStream.WriteLine(line);
            }
        }

        public static void ToStructuredLogFile(this string line)
        {
            if (!Directory.Exists("Output"))
            {
                try
                {
                    Directory.CreateDirectory("Output");
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            if (Directory.Exists("Output"))
            {
                line.ToFile($"Output/{DateTime.Now.Date:yyyy-MM-dd}.StructuredErrors.csv");
            }
        }
    }
}