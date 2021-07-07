using FluentAssertions;
using NUnit.Framework;
using Tolltech.Common;

namespace Tolltech.EnnoblerTest
{
    public class MetricsTest : TestBase
    {
        [Test]
        [TestCase(new[] {1, 2, 3, 4, 5, 6, 7}, 100, 7)]
        [TestCase(new[] {1, 2, 3, 4, 5, 6, 7}, 50, 3)]
        [TestCase(new[] {1, 2, 3, 4, 5, 6, 7, 8}, 50, 4)]
        [TestCase(new[] {1, 2, 3, 4, 5, 6, 7, 8}, 0, 0)]
        public void Test(int[] source, decimal percentile, int expected)
        {
            source.GetPercentile(percentile).Should().Be(expected);
        }
    }
}