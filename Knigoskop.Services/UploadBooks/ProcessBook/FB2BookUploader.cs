using Knigoskop.DataModel;
using Knigoskop.Services.FB2Engine;
using Knigoskop.Services.Logger;
using System;
using System.IO;
using System.Text;

namespace Knigoskop.Services.ProcessBook
{
    public class FB2BookUploader
    {
        private const int MINIMAL_IMAGE_SIZE = 4000;
        private const int DEFAULT_PUBLISH_YEAR = 1900;

        private Book book;
        private string fb2FileName;
        private Entities context;
        FB2Parser fb2Parser;

        public FB2BookUploader(Book book, string fb2FileName, Entities context)
        {
            this.book = book;
            this.fb2FileName = fb2FileName;
            this.context = context;
            if (IsFB2ParserValid())
            {
                LoadBookContent();
                LoadBookDescription();
                LoadBookTranslators();
                LoadBookPublisher();
                LoadBookMedia();
            }
        }

        private void LoadBookMedia()
        {
            if (fb2Parser.TitleInfo.CoverPage != null && fb2Parser.TitleInfo.CoverPage.Length > MINIMAL_IMAGE_SIZE)
            {
                book.Cover = fb2Parser.TitleInfo.CoverPage;
            }
            if (fb2Parser.BookBinaries != null && fb2Parser.BookBinaries.Length > 0)
            {
                foreach (FB2Binary binary in fb2Parser.BookBinaries)
                {
                    if (binary.Sources.Length > MINIMAL_IMAGE_SIZE)
                    {
                        BookMedia bookMedia = new BookMedia();
                        bookMedia.BookMediaId = Guid.NewGuid();
                        bookMedia.BookId = book.BookId;
                        bookMedia.MediaType = 1;
                        bookMedia.Content = binary.Sources;
                        bookMedia.MediaUpdated = DateTime.Now;
                        context.BookMedias.Add(bookMedia);
                    }
                }
            }
        }

        private void LoadBookPublisher()
        {
            if (!string.IsNullOrEmpty(fb2Parser.PublishInfo.Publisher))
            {
                book.Publisher = fb2Parser.PublishInfo.Publisher;
                if (fb2Parser.PublishInfo.Year > 0)
                {
                    book.Published = new DateTime(fb2Parser.PublishInfo.Year, 1, 1);
                }
                else
                {
                    book.Published = new DateTime(DEFAULT_PUBLISH_YEAR, 1, 1);
                }
                if (fb2Parser.PublishInfo.ISBNs != null)
                {
                    book.ISBN = fb2Parser.PublishInfo.ISBNs[0].Replace("-", "");
                }
            }
        }

        private void LoadBookDescription()
        {
            if (!string.IsNullOrEmpty(fb2Parser.TitleInfo.Annotation))
            {
                book.Description = fb2Parser.TitleInfo.Annotation;
            }
        }

        private void LoadBookTranslators()
        {
            if (!string.IsNullOrEmpty(fb2Parser.TitleInfo.Translator.FullName))
            {
                AuthorLoader authorLoader = new AuthorLoader(fb2Parser.TitleInfo.Translator.FullName, context);
                if (authorLoader.Authors != null)
                {
                    TranslatorsInBook translator = new TranslatorsInBook();
                    translator.TranslatorId = Guid.NewGuid();
                    translator.BookId = book.BookId;
                    translator.AuthorId = authorLoader.Authors[0].AuthorId;
                    context.TranslatorsInBooks.Add(translator);
                }
            }
        }

        private void LoadBookContent()
        {
            BookContent bookContent = new BookContent();
            bookContent.BookContentId = Guid.NewGuid();
            bookContent.BookId = book.BookId;
            bookContent.Content = fb2Parser.GetBookSources(Encoding.Unicode);
            bookContent.ContentType = ".txt";
            bookContent.IsOwnContent = false;
            bookContent.SourceFileName = Path.GetFileNameWithoutExtension(fb2FileName) + ".fb2";
            FileInfo fb2FileInfo = new FileInfo(fb2FileName);
            bookContent.Size = (int)fb2FileInfo.Length;
            if (fb2Parser.Fb2BookId != Guid.Empty)
            {
                bookContent.InternalBookId = fb2Parser.Fb2BookId.ToString();
            }
            bookContent.Source = fb2Parser.Fb2Sources;
            context.BookContents.Add(bookContent);
        }

        private bool IsFB2ParserValid()
        {
            try
            {
                fb2Parser = new FB2Parser(fb2FileName);
            }
            catch
            {
                ApplicationLogger.WriteStringToError("File " + fb2FileName + " is corrupted.");
                throw new Exception("File " + fb2FileName + " is corrupted.");
            }
            if (!string.IsNullOrEmpty(fb2Parser.BookText))
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
