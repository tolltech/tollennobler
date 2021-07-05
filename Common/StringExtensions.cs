namespace Tolltech.Common
{
    public static class StringExtensions
    {
        public static int? ToInt(this string src)
        {
            return int.TryParse(src, out var r) ? r : (int?)null;
        }

        public static int ToIntDefault(this string src, int defaultValue = 0)
        {
            return src.ToInt() ?? defaultValue;
        }
    }
}