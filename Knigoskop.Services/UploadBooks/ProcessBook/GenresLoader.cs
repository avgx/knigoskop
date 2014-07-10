using Knigoskop.DataModel;
using Knigoskop.Services.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Knigoskop.Services.ProcessBook
{
    public class GenresLoader
    {
        private const string DEFAULT_GENRE_NAME = "Прочее";
        private InpBookRecord bookRecord;
        private Entities context;
        private List<Genre> genres;

        public Genre[] Genres
        {
            get
            {
                if (genres != null)
                {
                    return genres.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        public GenresLoader(InpBookRecord bookRecord, Entities context)
        {
            this.bookRecord = bookRecord;
            this.context = context;
            ProcessGenres(bookRecord.Genres);
            if (genres == null || genres.Count == 0)
            {
                Genre defaultGenre;
                if (IsGenreExist(DEFAULT_GENRE_NAME, out defaultGenre))
                {
                    if (genres == null)
                    {
                        genres = new List<Genre>();
                    }
                    genres.Add(defaultGenre);
                }
            }
        }

        private void ProcessGenres(string[] genres)
        {
            foreach (string genre in genres)
            {
                if (!string.IsNullOrEmpty(genre))
                {
                    ProcessGenre(genre);
                }
            }
        }

        private void ProcessGenre(string genreName)
        {
            Genre genre;
            if (!IsGenreExist(genreName, out genre))
            {
                ApplicationLogger.WriteStringToError("Couldn't found this genre: \"" + genreName + "\". You should add this genre to a database before loading books.");
            }
            else
            {
                if (genres == null)
                {
                    genres = new List<Genre>();
                }
                genres.Add(genre);
            }
        }

        private bool IsGenreExist(string genreName, out Genre genre)
        {
            Genre resultGenre = context.Genres.FirstOrDefault(x => x.Classifier.ToLower() == genreName.ToLower());
            if (resultGenre != null)
            {
                genre = resultGenre;
                return true;
            }
            else
            {
                genre = null;
                return false;
            }
        }
    }
}
