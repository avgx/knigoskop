using Knigoskop.DataModel;
using Knigoskop.Services.Logger;
using System;
using System.IO;

namespace Knigoskop.Services.ProcessBook
{
    public class BookUploader
    {
        private BookSources bookSources;

        public BookUploader(BookSources bookSources)
        {
            this.bookSources = bookSources;
            UploadBookAndEntities();
        }

        private void UploadBookAndEntities()
        {
            using (Entities context = new Entities())
            {
                try
                {
                    AuthorLoader authorLoader = new AuthorLoader(bookSources.BookRecord, context);
                    if (authorLoader.Authors != null)
                    {
                        GenresLoader genresLoader = new GenresLoader(bookSources.BookRecord, context);
                        SeriesLoader seriesLoader = new SeriesLoader(bookSources.BookRecord, context);
                        InpBookLoader bookLoader = new InpBookLoader(bookSources.BookRecord, authorLoader, genresLoader, seriesLoader, context);
                        if (bookLoader.Book != null)
                        {
                            try
                            {
                                FB2BookUploader fb2Loader = new FB2BookUploader(bookLoader.Book, bookSources.BookFileName, context);
                                try
                                {
                                    LoadAuthorsDataFromWiki(context, bookLoader.Book.Authors);
                                }
                                catch { }
                                context.SaveChanges();
                                ApplicationLogger.WriteStringToLog("Book \"" + bookLoader.Book.Name + "\" has been uploaded successfully.");
                            }
                            catch (Exception ex)
                            {
                                ApplicationLogger.WriteStringToLog("Book from file: \"" + Path.GetFileName(bookSources.BookFileName) + "\" has not been uploaded because: \r\n" + ex.Message);
                            }
                        }
                        else
                        {
                            ApplicationLogger.WriteStringToLog("Book from file: \"" + Path.GetFileName(bookSources.BookFileName) + "\" has not been uploaded because exist in the database.");
                        }
                    }
                    else
                    {
                        ApplicationLogger.WriteStringToLog("Book from file: \"" + Path.GetFileName(bookSources.BookFileName) + "\" has not been uploaded because couldn't parse authors.");
                    }
                }
                catch (Exception ex)
                {
                    //transaction.Rollback();
                    throw new Exception("UploadBookAndEntities:\r\n" + ex.Message);
                }
            }
        }

        private void LoadAuthorsDataFromWiki(Entities context, System.Collections.Generic.ICollection<Author> authors)
        {
            foreach (Author author in authors)
            {
                new AuthorsDataFromWiki(context, author);
            }
        }
    }
}