using Knigoskop.DataModel;
using System;
using System.Linq;

namespace Knigoskop.Services.ProcessBook
{
    public partial class InpBookLoader
    {
        private InpBookRecord bookRecord;
        private AuthorLoader authorLoader;
        private GenresLoader genresLoader;
        private SeriesLoader seriesLoader;
        private Entities context;
        private Book book;

        public Book Book
        {
            get { return book; }
        }

        public InpBookLoader(InpBookRecord bookRecord, AuthorLoader authorLoader, GenresLoader genresLoader, SeriesLoader seriesLoader, Entities context)
        {
            this.bookRecord = bookRecord;
            this.authorLoader = authorLoader;
            this.genresLoader = genresLoader;
            this.seriesLoader = seriesLoader;
            this.context = context;
            ProcessBook();
        }

        private void ProcessBook()
        {
            string bookName = bookRecord.BookName;
            bookName = TextNormalization.NormalizeBookName(bookName);
            if (!FindBook(bookName, authorLoader.Authors))
            {
                AddBookToDatabase(bookName);
                InsertBookAuthors();
                InsertBookGenres();
                InsertBookSeries();
            }
        }

        private void InsertBookSeries()
        {
            if (seriesLoader.SeriaId != Guid.Empty)
            {
                BookInSerie bookInSerie = new BookInSerie();
                bookInSerie.BookId = book.BookId;
                bookInSerie.SerieId = seriesLoader.SeriaId;
                bookInSerie.Position = seriesLoader.SeriesPosition;
                context.BookInSeries.Add(bookInSerie);
            }
        }

        private void InsertBookGenres()
        {
            if (genresLoader.Genres != null)
            {
                foreach (Genre genre in genresLoader.Genres)
                {
                    book.Genres.Add(genre);
                }
            }
        }

        private void InsertBookAuthors()
        {
            foreach (Author author in authorLoader.Authors.GroupBy(item => item).Select(group => group.Key).ToList())
            {
                book.Authors.Add(author);
            }
        }

        private void AddBookToDatabase(string bookName)
        {
            book = new Book();
            book.BookId = Guid.NewGuid();
            book.Name = bookName;
            book.ClearName = TextNormalization.GetClearName(bookName);
            book.CoverUpdated = DateTime.Now;
            book.Created = DateTime.Now;
            context.Books.Add(book);
        }

        private bool FindBook(string bookName, Author[] authors)
        {
            Guid[] authorsIds = authors.Select(x => x.AuthorId).ToArray();
            //var exist = context.Books.Any(b => b.Name.ToLower() == bookName.ToLower() && b.Authors.Any(a => authors.Contains(a)));
            var exist = context.Books.Any(b => b.Name.ToLower() == bookName.ToLower() && b.Authors.Any(a => authorsIds.Contains(a.AuthorId)));
            return exist;
        }
    }
}
