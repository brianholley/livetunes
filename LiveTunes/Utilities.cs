using System;
using System.Text.RegularExpressions;

namespace LiveTunes
{
    public class Utilities
    {
        public static string StripHTML(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            Regex brRegex = new Regex(@"<\s*br\s*/?>", RegexOptions.IgnoreCase);
            string output = brRegex.Replace(input, "\n");

            Regex tagRegex = new Regex(@"<.+?>", RegexOptions.IgnoreCase);
            output = tagRegex.Replace(output, "");

            output = output.Replace("&lt;", "<");
            output = output.Replace("&gt;", ">");
            output = output.Replace("&amp;", "&");

            return output;
        }
    }
}
