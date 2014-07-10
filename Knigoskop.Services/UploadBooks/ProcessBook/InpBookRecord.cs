using System.Collections.Generic;

namespace Knigoskop.Services.ProcessBook
{
    public class InpBookRecord
    {
        private const char ENTITIES_SEPARATOR = '';
        private const char ITEMS_SEPARATOR = ':';

        private string bookRecord;
        private string[] authors;
        private string[] genres;
        private string seria;
        private int seriesPosition;
        private int bookNumber;
        private string bookFormat;
        private string bookLanguage;
        private string bookName;

        public string BookName
        {
            get { return bookName; }
        }

        public string BookLanguage
        {
            get { return bookLanguage; }
        }

        public string BookFormat
        {
            get { return bookFormat; }
        }

        public int BookNumber
        {
            get { return bookNumber; }
        }

        public int SeriesPosition
        {
            get { return seriesPosition; }
        }

        public string Seria
        {
            get { return seria; }
        }

        public string[] Genres
        {
            get
            {
                genres = PrepareArray(genres);
                if (genres != null && genres.Length > 0)
                {
                    return genres;
                }
                else
                {
                    return null;
                }
            }
        }

        public string[] Authors
        {
            get
            {
                authors = PrepareArray(authors);
                if (authors != null && authors.Length > 0)
                {
                    return authors;
                }
                else
                {
                    return null;
                }
            }
        }

        private string[] PrepareArray(string[] stringArray)
        {
            List<string> stringList = new List<string>();
            foreach (string str in stringArray)
            {
                if (!string.IsNullOrEmpty(str.Trim()))
                {
                    stringList.Add(str);
                }
            }
            return stringList.ToArray();
        }

        public InpBookRecord(string bookRecord)
        {
            this.bookRecord = bookRecord;
            ParseBookRecord();
        }

        private void ParseBookRecord()
        {
            string[] parts = bookRecord.Split('');
            authors = parts[0].Split(':');
            genres = parts[1].ToLower().Split(':');
            bookName = parts[2];
            string seria = parts[3].Trim();
            if (!string.IsNullOrEmpty(seria) && !string.IsNullOrEmpty(parts[4]))
            {
                this.seria = seria;
                int.TryParse(parts[4], out seriesPosition);
            }
            string fileNumberStr = parts[5];
            int.TryParse(fileNumberStr, out bookNumber);
            bookFormat = parts[9].ToLower();
            bookLanguage = parts[11].ToLower();
        }
    }
}
