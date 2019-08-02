using System.Linq;

namespace Ginseng.Models.Extensions
{
    public static class StringExtensions
    {
        public static int ParseInt(this string input)
        {
            try
            {
                string result = string.Join("", input.Where(c => char.IsNumber(c)));
                return int.Parse(result);
            }
            catch 
            {
                return 0;
            }
        }
    }
}
