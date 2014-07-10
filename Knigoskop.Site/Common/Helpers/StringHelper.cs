using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Knigoskop.Site.Common.Helpers
{
    public static class StringHelper
    {
        public static string Declension(int number, string nominative, string genitiveSingular, string genitivePlural)
        {
            int lastDigit = number%10;
            int lastTwoDigits = number%100;
            if (lastDigit == 1 && lastTwoDigits != 11)
            {
                return nominative;
            }
            if (lastDigit == 2 && lastTwoDigits != 12 || lastDigit == 3 && lastTwoDigits != 13 ||
                lastDigit == 4 && lastTwoDigits != 14)
            {
                return genitiveSingular;
            }
            return genitivePlural;
        }

        public static string TrimText(this string text, int maxCharacters, string trailingStringIfTextCut = "...")
        {
            if (text == null || (text = text.Trim()).Length <= maxCharacters) return text;
            int trailLength = trailingStringIfTextCut.StartsWith("&") ? 1 : trailingStringIfTextCut.Length;
            maxCharacters = maxCharacters - trailLength >= 0 ? maxCharacters - trailLength : 0;
            int pos = text.LastIndexOf(" ", maxCharacters, StringComparison.Ordinal);
            if (pos >= 0)
            {
                text = text.Substring(0, pos);
                //if (!string.IsNullOrEmpty(trailingStringIfTextCut))
                text = text.TrimEnd('.', ',', '!', '?', ';', ':');
                return text + trailingStringIfTextCut;
            }
            return string.Empty;
        }

        public static string ToHtml(this string text)
        {
            return text.Split('\n').Aggregate<string, string>(null, (current, line) => current + ("<p>" + line + "</p>"));
        }

        public static string HtmlToPlainText(this string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
    }
}