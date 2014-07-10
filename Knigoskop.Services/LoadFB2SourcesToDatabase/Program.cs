using Knigoskop.DataModel;
using Knigoskop.Services.Logger;
using Knigoskop.Services.ZipFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Knigoskop.Services
{
    class Program
    {
        private static List<string> bookArchives;
        static void Main(string[] args)
        {
            ApplicationLogger.ClearLogFile();
            CreateAndClearTempFolder();
            ProcessBooks();
        }

        private static void ProcessBooks()
        {
            List<Guid> books = null;
            using (Entities context = new Entities())
            {
                books = context.Flags.Where(x => x.Name.ToLower().Equals("sources uploaded") && x.State == 0).Select(x => (Guid)x.BookId).ToList();
            }
            foreach (Guid bookId in books)
            {
                ProcessBook(bookId, 1);
            }
        }

        private static void CreateAndClearTempFolder()
        {
            if (!Directory.Exists(AppConfig.TemporaryFolder))
            {
                Directory.CreateDirectory(AppConfig.TemporaryFolder);
            }
            else
            {
                foreach (string file in Directory.GetFiles(AppConfig.TemporaryFolder))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void ProcessBook(Guid bookID, int iterator)
        {
            try
            {
                using (Entities context = new Entities())
                {
                    Book book = context.Books.FirstOrDefault(x => x.BookId == bookID);
                    if (book.BookContents.Count > 0)
                    {
                        ApplicationLogger.WriteStringToLog("Process book: " + book.BookId.ToString());
                        Flag flag = context.Flags.FirstOrDefault(x => x.Name.ToLower().Equals("sources uploaded") && x.BookId == bookID);
                        UploadBookSources(book, flag);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                if (iterator <= 10)
                {
                    ApplicationLogger.WriteStringToError(e.Message);
                    Thread.Sleep(2000);
                    iterator = iterator++;
                    ProcessBook(bookID, iterator);
                }
                else
                {
                    ApplicationLogger.WriteStringToLog("Application can't commit cahnges to the database because:\r\n");
                    throw e;
                }
            }
        }

        private static void UploadBookSources(Book book, Flag flag)
        {
            byte[] bookSources = GetBookSources(book.BookContents.FirstOrDefault().SourceFileName);
            /*using (FileStream fs = new FileStream(AppConfig.TemporaryFolder + "test.zip", FileMode.Create, FileAccess.Write))
            {
                fs.Write(bookSources, 0, bookSources.Length);
                fs.Flush();
            }*/
            if (bookSources != null)
            {
                book.BookContents.FirstOrDefault().Source = bookSources;
                flag.State = 1;
            }
        }

        private static byte[] GetBookSources(string fb2FileName)
        {
            try
            {
                FB2ZipMethods.UnZipFB2File(GetArchiveFileName(fb2FileName), fb2FileName, AppConfig.TemporaryFolder);
            }
            catch
            {
                return null;
            }
            if (File.Exists(AppConfig.TemporaryFolder + fb2FileName))
            {
                try
                {
                    return FB2ZipMethods.ZipFB2File(AppConfig.TemporaryFolder + fb2FileName);
                }
                finally
                {
                    File.Delete(AppConfig.TemporaryFolder + fb2FileName);
                }
            }
            {
                return null;
            }
        }

        private static string GetArchiveFileName(string fb2FileName)
        {
            if (bookArchives == null)
            {
                bookArchives = new List<string>();
                foreach (string fileName in Directory.GetFiles(AppConfig.BooksFolder, "fb2-??????-??????.zip"))
                {
                    bookArchives.Add(Path.GetFileNameWithoutExtension(fileName));
                }
            }
            string result = string.Empty;
            int fb2FileIdx = int.Parse(Path.GetFileNameWithoutExtension(fb2FileName));
            foreach (string fileName in bookArchives)
            {
                int firstFile = int.Parse(fileName.Substring(4, 6));
                int lastFile = int.Parse(fileName.Substring(11, 6));
                if (fb2FileIdx >= firstFile && fb2FileIdx <= lastFile)
                {
                    result = AppConfig.BooksFolder + fileName + ".zip";
                    break;
                }
            }


            return result;
        }

        private static bool BookCanBeDownloaded(Book book)
        {
            bool result = true;
            int maxPeriodFromDeath = 0;
            foreach (Author author in book.Authors)
            {
                int periodFromDeath = PeriodFromDeath(author.DeathDate);
                if (periodFromDeath > maxPeriodFromDeath)
                {
                    maxPeriodFromDeath = periodFromDeath;
                }
            }
            if (maxPeriodFromDeath < 71)
            {
                return false;
            }
            if (book.BookContents.FirstOrDefault() != null)
            {
                return result && book.BookContents.FirstOrDefault().Source == null;
            }
            else
            {
                return false;
            }
        }

        private static int PeriodFromDeath(DateTime? deathDate)
        {
            if (deathDate == null)
            {
                return 0;
            }
            {
                return DateTime.Now.Year - deathDate.Value.Year;
            }
        }
    }
}
