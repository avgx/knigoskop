using Knigoskop.Services.Logger;
using Knigoskop.Services.ProcessBook;
using Knigoskop.Services.ZipFunctions;
using System;
using System.IO;

namespace Knigoskop.Services.UploadBooks
{
    public class LibraryUpdater
    {
        private int lastBookIdFromDatabase;

        public LibraryUpdater(int lastBookIdFromDatabase)
        {
            this.lastBookIdFromDatabase = lastBookIdFromDatabase;
            ProcessUpdates();
        }

        private void ProcessUpdates()
        {
            foreach (string file in Directory.GetFiles(AppConfig.TemporaryFolder, "*.inp"))
            {
                ApplicationLogger.WriteStringToLog("Start processing file: " + Path.GetFileName(file));
                ProcessLibraryFile(file);
                ApplicationLogger.WriteStringToLog("Finished process file: " + Path.GetFileName(file));
            }
        }

        private void ProcessLibraryFile(string libraryFile)
        {
            string fileStartBookIdStr = Path.GetFileName(libraryFile).Substring(4, 6);
            string fileFinishBookIdStr = Path.GetFileName(libraryFile).Substring(11, 6);
            int fileStartBookId;
            int fileFinishBookId;
            int.TryParse(fileStartBookIdStr, out fileStartBookId);
            int.TryParse(fileFinishBookIdStr, out fileFinishBookId);
            if ((lastBookIdFromDatabase < fileStartBookId) || ((lastBookIdFromDatabase >= fileStartBookId) && (lastBookIdFromDatabase < fileFinishBookId)))
            {
                using (FileStream fs = new FileStream(libraryFile, FileMode.Open, FileAccess.Read))
                {
                    using (TextReader tr = new StreamReader(fs))
                    {
                        String line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            ProcessBookLine(libraryFile, line);
                        }
                    }
                }
            }
        }

        private void ProcessBookLine(string libraryFile, string line)
        {
            InpBookRecord bookRecord = new InpBookRecord(line);
            if (bookRecord.BookNumber > lastBookIdFromDatabase && bookRecord.BookLanguage.Equals("ru") && bookRecord.BookFormat.Equals("fb2"))
            {
                BookSources bookSources = new BookSources(bookRecord, GetBookFileName(libraryFile, bookRecord.BookNumber));
                if (File.Exists(bookSources.BookFileName))
                {
                    ApplicationLogger.WriteStringToLog("Start processing book: " + Path.GetFileName(bookSources.BookFileName));
                    try
                    {
                        BookUploader bookUploader = new BookUploader(bookSources);
                    }
                    finally
                    {
                        File.Delete(bookSources.BookFileName);
                    }
                    ApplicationLogger.WriteStringToLog("Finished process book: " + Path.GetFileName(bookSources.BookFileName));
                }
            }
        }

        private string GetBookFileName(string libraryFile, int fileNumber)
        {
            string zipFileName = AppConfig.BooksFolder + Path.GetFileNameWithoutExtension(libraryFile) + ".zip";
            if (!File.Exists(zipFileName))
            {
                ApplicationLogger.WriteStringToError("File " + zipFileName + " doesn't exist.");
                throw new Exception("File " + zipFileName + " doesn't exist.");
            }
            FB2ZipMethods.UnZipFB2File(zipFileName, fileNumber.ToString() + ".fb2", AppConfig.TemporaryFolder);
            string result = AppConfig.TemporaryFolder + "\\" + fileNumber.ToString() + ".fb2";
            if (!File.Exists(result))
            {
                ApplicationLogger.WriteStringToError("File " + result + " doesn't exist.");
            }
            return result;
        }

    }
}
