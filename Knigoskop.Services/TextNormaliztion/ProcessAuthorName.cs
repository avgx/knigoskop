using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knigoskop.Services
{
    public partial class TextNormalization
    {
        private const string letters = "abcdefghijklmnopqrstuvwxyzабвгдеёжзийклмнопрстуфхцчшщъыьэюя?";
        private static string[] predefinedAuthors = { "журнал", "Е Цзы", "Е Шэн-Тао", "У Хань", "У Цзу-сян", "У Цзы", "У Чэн-энь", 
                                                 "Ю Во","У Вэйсинь" };
        public static string NormalizeAuthor(string author)
        {
            author = author.Trim();
            if (!IsPredefinedAuthor(author))
            {
                author = RemoveEndCommas(author);
                author = RemoveCommas(author);
                author = SmartAuthorTransformation(author);
            }
            return author;
        }

        private static bool IsPredefinedAuthor(string author)
        {
            foreach (string predefined in predefinedAuthors)
            {
                if (author.ToLower().Contains(predefined.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private static string RemoveCommas(string author)
        {
            string[] parts = null;
            if (author.Contains(','))
            {
                parts = author.Split(',');
            }
            else
            {
                parts = author.Split(' ');
            }
            author = string.Empty;
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part.Trim()))
                {
                    author += part.Replace("?", "") + " ";
                }
            }
            return author.Trim();
        }

        private static string SmartAuthorTransformation(string author)
        {
            string result = string.Empty;
            string[] parts = author.Trim().Split(' ');
            parts = RemoveEndPointFromParts(parts);
            parts = ReplaceLat2Rus(parts);
            //parts = NormalizeLettersCase(parts);
            string nameTemplate = getNameTemplate(parts);
            switch (nameTemplate)
            {
                case "N11":
                case "NN1":
                case "N1":
                    {
                        result = ConstructNameFromParts(parts);
                        break;
                    }
                case "1N1":
                case "11N":
                case "1N":
                    {
                        parts = MoveInitialParts(parts, nameTemplate);
                        result = ConstructNameFromParts(parts);
                        break;
                    }
                default:
                    {
                        result = author;
                        break;
                    }
            }
            return result;
        }

        private string[] NormalizeLettersCase(string[] parts)
        {
            List<string> result = new List<string>();
            foreach (string part in parts)
            {
                string resultPart = string.Empty;
                bool toUpper = true;
                for (int i = 0; i < part.Length; i++)
                {
                    if (part[i].Equals('\'') || part[i].Equals('`') || (part[i].Equals('-') && part.Length > 3))
                    {
                        resultPart += part[i];
                        toUpper = true;
                    }
                    else
                    {
                        if (toUpper)
                        {
                            resultPart += part[i].ToString().ToUpper();
                            toUpper = false;
                        }
                        else
                        {
                            resultPart += part[i].ToString().ToLower();
                        }
                    }
                }
                result.Add(resultPart);
            }
            return result.ToArray();
        }

        private static string[] MoveInitialParts(string[] parts, string nameTemplate)
        {
            string[] result = new string[parts.Length];
            switch (nameTemplate)
            {
                case "11N":
                    {
                        result[0] = parts[2];
                        result[1] = parts[0];
                        result[2] = parts[1];
                        break;
                    }
                case "1N1":
                    {
                        result[0] = parts[1];
                        result[1] = parts[2];
                        result[2] = parts[0];
                        break;
                    }
                case "1N":
                    {
                        result[0] = parts[1];
                        result[1] = parts[0];
                        break;
                    }
            }
            return result;
        }

        private static string ConstructNameFromParts(string[] parts)
        {
            string result = parts[0] + " ";
            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].Length == 1 && !IsDigit(parts[i]))
                {
                    parts[i] += ".";
                }
                result += parts[i] + " ";
            }
            return result.Trim();
        }

        private static bool IsDigit(string word)
        {
            int digit;
            return int.TryParse(word, out digit);
        }

        private static string getNameTemplate(string[] parts)
        {
            string result = string.Empty;
            foreach (string part in parts)
            {
                int length = part.Length;
                if (length == 1)
                {
                    result += "1";
                }
                else
                {
                    result += "N";
                }
            }
            return result;
        }

        private static string[] RemoveEndPointFromParts(string[] parts)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].EndsWith("."))
                {
                    parts[i] = parts[i].Replace(".", "");
                }
            }
            return parts;
        }

        private static string RemoveEndCommas(string author)
        {
            if (author.EndsWith(","))
            {
                author = author.Replace(",", "");
                author = RemoveEndCommas(author);
            }
            return author.Trim();
        }

        private static string[] ReplaceLat2Rus(string[] parts)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (!IsRomanDigit(parts[i]))
                {
                    parts[i] = ReplaceLat2Rus(parts[i]);
                }
            }
            return parts;
        }

        private static bool IsRomanDigit(string word)
        {
            string[] romanDigits = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
            foreach (string romanDigit in romanDigits)
            {
                if (word.ToUpper().Equals(romanDigit))
                {
                    return true;
                }
            }
            return false;
        }
        private static string ReplaceLat2Rus(string author)
        {
            string acceptableChars = "АаБбВвГгДдЕеЁёЖжЗзИиЙйКкЛлМмНнОоПпРрСсТтУуФфХхЦцЧчШшЩщЪъЫыЬьЭэЮюЯя -.'`\"”“[]’‘{}?<>()©«»LlDdYSsNnhk­­fZzJ/_:V&!Іі";
            Dictionary<string, string> m = new Dictionary<string, string>();
            m.Add("B", "В");
            m.Add("e", "е");
            m.Add("E", "Е");
            m.Add("c", "С");
            m.Add("C", "С");
            m.Add("H", "Н");
            m.Add("A", "А");
            m.Add("a", "а");
            m.Add("X", "Х");
            m.Add("I", "І");
            m.Add("і", "і");
            m.Add("O", "О");
            m.Add("M", "М");
            m.Add("o", "о");
            m.Add("u", "и");
            m.Add("m", "т");
            m.Add("P", "Р");
            m.Add("p", "р");
            m.Add("–", "-");
            m.Add("K", "к");
            m.Add("T", "Т");
            m.Add("—", "-");
            m.Add("x", "х");
            m.Add("y", "у");

            if (RusCoeff(author) <= 1 && RusCoeff(author) >= 0.6)
            {
                string tmpStr = string.Empty;
                int tmp;
                for (int i = 0; i < author.Length; i++)
                {
                    if (!acceptableChars.Contains(author[i]) && !int.TryParse(author[i].ToString(), out tmp))
                    {
                        try
                        {
                            tmpStr += m[author[i].ToString()];
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        tmpStr += author[i];
                    }
                }
                author = tmpStr;
            }
            return author;
        }

        public static float RusCoeff(string text)
        {
            if (text.Length > 0)
            {
                string acceptableChars = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя .,";
                float rusChars = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (acceptableChars.Contains(text.ToLower()[i]))
                    {
                        rusChars++;
                    }
                }
                return rusChars / text.Length;
            }
            else return 1;
        }

        private static bool IsLetterAndPoint(string part)
        {
            bool result = false;
            if (part.Length == 2 && part[1].Equals('.') && letters.Contains(part.ToLower()[0]))
            {
                result = true;
            }
            return result;
        }
    }
}
