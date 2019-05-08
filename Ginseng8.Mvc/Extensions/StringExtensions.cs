namespace Ginseng.Mvc.Extensions
{
    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Indicates whether the specified string is null or an <see cref="F:System.String.Empty"></see> string.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the <paramref name="value">value</paramref> parameter is null or an empty string (&amp;quot;&amp;quot;); otherwise, false.</returns>
        public static bool IsNullOrEmpty(this string value)
            => string.IsNullOrEmpty(value);
    }
}
