using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knigoskop.Services.ParseGoogleBooks
{
    public partial class ProcessBooks
    {
        private BookItem ChooseBookItem(GoogleBookJsonResponse booksDataFromGoogle, string bookName, string authorName)
        {
            BookItem bookItem = null;
            foreach (BookItem item in booksDataFromGoogle.items)
            {
                if (PhrasesAreEqual(bookName, item.volumeInfo.title) && AuthorIsInList(authorName, item.volumeInfo.authors))
                {
                    bookItem = item;
                    break;
                }
            }
            return bookItem;
        }

        private bool AuthorIsInList(string authorName, string[] authors)
        {
            if (authors != null)
            {
                foreach (string author in authors)
                {
                    if (PhrasesAreEqual(authorName, author))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        private bool PhrasesAreEqual(string phrase1, string prase2)
        {
            if (!phrase1.ToLower().Equals(prase2.ToLower()))
            {
                phrase1 = RemoveUselessChars(phrase1);
                prase2 = RemoveUselessChars(prase2);
                string[] phrase1Parts = phrase1.Split(' ');
                string[] phrase2Parts = prase2.Split(' ');
                if (phrase1Parts.Length != phrase2Parts.Length)
                {
                    return false;
                }
                int[] match = new int[phrase1Parts.Length];
                for (int i = 0; i < phrase1Parts.Length; i++)
                {
                    for (int j = 0; j < phrase2Parts.Length; j++)
                    {
                        if (WordsAreEqual(phrase1Parts[i], phrase2Parts[j]))
                        {
                            match[i] = 1;
                            break;
                        }
                    }
                }
                if (match.Sum() / phrase1Parts.Length >= 0.9)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool WordsAreEqual(string word1, string word2)
        {
            word1 = word1.ToLower();
            word2 = word2.ToLower();
            if (!word1.Equals(word2))
            {
                int matching = 0;
                for (int i = 0; i < word1.Length; i++)
                {
                    if (word2.Contains(word1[i]))
                    {
                        matching++;
                    }
                }
                if (word1.Length > 0 && matching / word1.Length > 0.8)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private string RemoveUselessChars(string phrase)
        {
            string uselessChars = ".,!\"?()";
            for (int i = 0; i < uselessChars.Length; i++)
            {
                phrase = phrase.Replace(uselessChars[i].ToString(), "");
            }
            return phrase;
        }
    }
}
