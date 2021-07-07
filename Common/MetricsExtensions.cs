using System;
using System.Linq;
using JetBrains.Annotations;

namespace Tolltech.Common
{
    public static class MetricsExtensions
    {
        public static T GetPercentile<T>([NotNull] this T[] metrics, decimal percentile)
        {
            if (percentile < 0 || percentile > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentile), percentile,
                    $"Argument is out of 0..100 range");
            }

            var takeCount = (int) (metrics.Length * percentile / 100m);

            return metrics.OrderBy(x => x).Take(takeCount).LastOrDefault();
        }

        public static T GetMedian<T>([NotNull] this T[] metrics)
        {
            return metrics.GetPercentile(50);
        }
    }
}