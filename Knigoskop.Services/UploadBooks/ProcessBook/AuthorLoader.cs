using Knigoskop.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Knigoskop.Services.ProcessBook
{
    public class AuthorLoader
    {
        private InpBookRecord bookRecord;
        private Entities context;
        private List<Author> authors;

        public Author[] Authors
        {
            get
            {
                if (authors != null && authors.Count > 0)
                {
                    return authors.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }


        public AuthorLoader(InpBookRecord bookRecord, Entities context)
        {
            this.bookRecord = bookRecord;
            this.context = context;
            ProcessAuthorsArray(bookRecord.Authors);
        }

        public AuthorLoader(string authorsRecord, Entities context)
        {
            this.context = context;
            string[] authors = authorsRecord.Split(':');
            ProcessAuthorsArray(authors);
        }

        private void ProcessAuthorsArray(string[] authors)
        {
            foreach (string authorStr in authors)
            {
                if (!string.IsNullOrEmpty(authorStr))
                {
                    string normalizedAuthor = authorStr.Replace(",", " ").Trim();
                    normalizedAuthor = TextNormalization.NormalizeAuthor(normalizedAuthor);
                    if ((TextNormalization.RusCoeff(normalizedAuthor) <= 1 && TextNormalization.RusCoeff(normalizedAuthor) >= 0.6))
                    {
                        if (!DatabaseContainsAuthor(normalizedAuthor.ToLowerInvariant()))
                        {
                            AddAuthorToDatabase(normalizedAuthor);
                        }
                        else
                        {

                            Author author = context.Authors.FirstOrDefault(x => x.Name == normalizedAuthor);
                            if (author != null)
                            {
                                Guid authorId = author.AuthorId;
                                if (this.authors == null)
                                {
                                    this.authors = new List<Author>();
                                }
                                this.authors.Add(author);
                            }
                        }
                    }
                }
            }
        }


        private void AddAuthorToDatabase(string authorName)
        {
            Author author = new Author();
            author.AuthorId = Guid.NewGuid();
            author.Name = authorName;
            author.PhotoUpdated = DateTime.Now;
            context.Authors.Add(author);
            if (this.authors == null)
            {
                this.authors = new List<Author>();
            }
            this.authors.Add(author);
        }

        private bool DatabaseContainsAuthor(string authorName)
        {
            Author author = context.Authors.FirstOrDefault(x => x.Name.ToLower() == authorName.ToLower());
            if (author != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
