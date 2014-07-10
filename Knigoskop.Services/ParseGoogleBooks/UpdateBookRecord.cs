using Knigoskop.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knigoskop.Services.ParseGoogleBooks
{
    public partial class ProcessBooks
    {
        private void UpdateBookRecord(Entities context, Book book, BookItem bookItem)
        {
            if (book.Cover == null && bookItem.volumeInfo.imageLinks != null && bookItem.volumeInfo.imageLinks.thumbnail != null)
            {
                book.Cover = DownloadCover(bookItem.volumeInfo.imageLinks.thumbnail);
                book.CoverUpdated = DateTime.Now;
            }
            if (string.IsNullOrEmpty(book.Description) && !string.IsNullOrEmpty(bookItem.volumeInfo.description))
            {
                book.Description = bookItem.volumeInfo.description;
            }
            if (book.PagesCount.GetValueOrDefault(0) == 0 && bookItem.volumeInfo.pageCount > 0)
            {
                book.PagesCount = bookItem.volumeInfo.pageCount;
            }
            if (!string.IsNullOrEmpty(bookItem.volumeInfo.publisher))
            {
                book.Publisher = bookItem.volumeInfo.publisher;
            }
            if (bookItem.volumeInfo.publishedDate != null)
            {
                try
                {
                    book.PublishDate = Convert.ToDateTime(bookItem.volumeInfo.publishedDate);
                }
                catch { }
            }
            if (bookItem.volumeInfo.industryIdentifiers != null)
            {
                book.ISBN = GetISBN(bookItem.volumeInfo.industryIdentifiers);
            }
            UpdateBookRecordAsProcessed(context, book);
        }

        private string GetISBN(Industryidentifier[] isbnArray)
        {
            string isbn = string.Empty;
            foreach (Industryidentifier item in isbnArray)
            {
                if (item.identifier.Length > isbn.Length)
                {
                    isbn = item.identifier;
                }
            }
            return isbn;
        }

        private byte[] DownloadCover(string coverUrl)
        {
            KnigoskopWebClient webClient = new KnigoskopWebClient();
            byte[] cover = webClient.DownloadImage(coverUrl);
            return cover;
        }

        private void UpdateBookRecordAsProcessed(Entities context, Book book)
        {
            book.FromGoogleBooks = true;
            if (book.CoverUpdated == null)
            {
                book.CoverUpdated = DateTime.Now;
            }
        }
    }
}
