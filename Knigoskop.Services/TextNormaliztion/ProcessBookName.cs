using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knigoskop.Services
{
    public partial class TextNormalization
    {
        public static string NormalizeBookName(string bookName)
        {
            bookName = RemoveQuotesFromBookName(bookName);

            return bookName;
        }

        private static string RemoveQuotesFromBookName(string bookName)
        {
            string newBookName = string.Empty;
            if (bookName.Contains('"') || bookName.Contains('“') || bookName.Contains('”') || bookName.Contains("«") || bookName.Contains("»"))
            {
                if (bookName.StartsWith("\"") && bookName.EndsWith("\"") && QuotesCount('"', bookName) == 2)
                {
                    newBookName = bookName.Replace("\"", "");
                }
                else if (bookName.StartsWith("\"") && bookName.EndsWith("\"") && bookName.Contains(" \"") && bookName.Contains("\" ") &&
                    bookName.IndexOf(" \"") < bookName.IndexOf("\" "))
                {
                    newBookName = bookName.Substring(1);
                    newBookName = newBookName.Substring(0, newBookName.Length - 1);
                }
                else if (bookName.StartsWith("“") && bookName.EndsWith("”"))
                {
                    newBookName = bookName.Substring(1);
                    newBookName = newBookName.Substring(0, newBookName.Length - 1);
                }
                else if (bookName.StartsWith("(«") && bookName.EndsWith("»)"))
                {
                    newBookName = bookName.Substring(2);
                    newBookName = newBookName.Substring(0, newBookName.Length - 2);
                }
                else if (bookName.StartsWith("«") && bookName.EndsWith("»") && (QuotesCount('«', bookName) + QuotesCount('»', bookName)) == 2)
                {
                    newBookName = bookName.Substring(1);
                    newBookName = newBookName.Substring(0, newBookName.Length - 1);
                }
                else if (bookName.StartsWith("«") && bookName.EndsWith("»") && bookName.Contains(" «") && bookName.Contains("» ") &&
                bookName.IndexOf(" «") < bookName.IndexOf("» "))
                {
                    newBookName = bookName.Substring(1);
                    newBookName = newBookName.Substring(0, newBookName.Length - 1);
                }
                else
                {
                    newBookName = bookName;
                }
            }
            else
            {
                newBookName = bookName;
            }
            return newBookName;
        }

        private static int QuotesCount(char quote, string bookName)
        {
            if (bookName.Contains(quote))
            {
                int quotesCount = 0;
                for (int i = 0; i < bookName.Length; i++)
                {
                    if (bookName[i] == quote)
                    {
                        quotesCount++;
                    }
                }
                return quotesCount;
            }
            else
            {
                return 0;
            }
        }

        public static string GetClearName(string bookName)
        {
            return RemoveCharsInText(bookName, " -\"ё,.!«»—[]()?:;…–'");
        }

        private static string RemoveCharsInText(string text, string chars)
        {
            string result = text.ToLower();
            result = result.Replace("ё", "е");
            for (int i = 0; i < chars.Length; i++)
            {
                result = result.Replace(chars[i].ToString(), "");
            }
            return result;
        }
    }
}
