using System;
using System.Text;

namespace Ginseng.Models.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces a substring of a string with a new string
        /// </summary>
        public static string ReplaceAt(this string input, int start, int length, string replaceWith)
        {
            // help from answers at https://stackoverflow.com/questions/5015593/how-to-replace-part-of-string-by-position
            // favorite answer was https://stackoverflow.com/a/5015636/2023653

            StringBuilder sb = new StringBuilder(input);
            sb.Remove(start, length);
            sb.Insert(start, replaceWith);
            return sb.ToString();
        }

        /// <summary>
        /// Replaces the n-th instance of search with a new string
        /// </summary>
        public static string ReplaceAtIndexOf(this string input, int indexOf, string search, string replaceWith)
        {
            int position = NthIndexOf(input, indexOf, search);
            if (position < 0) throw new InvalidOperationException($"Couldn't find index {indexOf} of {search} in {input}");
            return ReplaceAt(input, position, search.Length, replaceWith);
        }

        public static int NthIndexOf(this string input, int indexOf, string search)
        {
            int start = 0;
            int index = 0;
            int result = -1;
            while (index <= indexOf)
            {
                result = input.IndexOf(search, start);
                if (result < 0) return -1;
                index++;
                start = result + search.Length + 1;
            }
            return result;
        }
    }
}
