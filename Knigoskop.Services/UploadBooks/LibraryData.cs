using Knigoskop.DataModel;
using Knigoskop.Services.Logger;
using Knigoskop.Services.ZipFunctions;
using System;
using System.IO;
using System.Linq;

namespace Knigoskop.Services.UploadBooks
{
    public class LibraryData
    {
        private const string COLLECTION_FILE = "librusec_local_fb2.inpx";

        private int lastBookIdFromDatabase;

        private bool newBooksAreAvailable = false;

        public bool NewBooksAreAvailable
        {
            get { return newBooksAreAvailable; }
        }

        public int LastBookIdFromDatabase
        {
            get { return lastBookIdFromDatabase; }
        }

        public LibraryData()
        {
            CheckFolders();
            ClearTempFolder();
            PrepareCollectionInfo();
            VerifyNewDataAvailability();
            if (newBooksAreAvailable)
            {
                ApplicationLogger.WriteStringToLog("New books are available to upload to the database.");
            }
            else
            {
                ApplicationLogger.WriteStringToLog("New updates didn't find.");
            }
        }

        private void VerifyNewDataAvailability()
        {
            GetLastBookIdFromDatabase();
            foreach (string file in Directory.GetFiles(AppConfig.TemporaryFolder, "*.inp"))
            {
                string fileStartBookIdStr = Path.GetFileName(file).Substring(4, 6);
                string fileFinishBookIdStr = Path.GetFileName(file).Substring(11, 6);
                int fileStartBookId;
                int fileFinishBookId;
                int.TryParse(fileStartBookIdStr, out fileStartBookId);
                int.TryParse(fileFinishBookIdStr, out fileFinishBookId);
                if ((lastBookIdFromDatabase < fileStartBookId) || ((lastBookIdFromDatabase >= fileStartBookId) && (lastBookIdFromDatabase < fileFinishBookId)))
                {
                    newBooksAreAvailable = true;
                }
                else
                {
                    File.Delete(file);
                }
            }
        }

        private void GetLastBookIdFromDatabase()
        {
            using (Entities context = new Entities())
            {
                int maxFileId = context.BookContents.Select(s => s.SourceFileName.Replace(".fb2", "")).ToList().Max(n=>int.Parse(n));
                lastBookIdFromDatabase = maxFileId;
                ApplicationLogger.WriteStringToLog("Last uploaded book Id = " + lastBookIdFromDatabase.ToString());
            }
        }

        private void ClearTempFolder()
        {
            ApplicationLogger.WriteStringToLog("Clear temporary folder.");
            foreach (string file in Directory.GetFiles(AppConfig.TemporaryFolder))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    throw new Exception("ClearTempFolder\r\n" + e.Message);
                }
            }
        }

        private void PrepareCollectionInfo()
        {
            string collectionFile = AppConfig.LibraryFolder + COLLECTION_FILE;
            if (!File.Exists(collectionFile))
            {
                throw new ApplicationException("Colection file doesn't exist: " + collectionFile);
            }
            ApplicationLogger.WriteStringToLog("Unzip collection file to temporary folder.");
            UnzipFeatures.UnzipFileToFolder(collectionFile, AppConfig.TemporaryFolder);
        }

        private void CheckFolders()
        {
            if (!CheckFolderAvailability(AppConfig.LibraryFolder, false))
            {
                throw new ApplicationException("Can't find folder: \"" + AppConfig.LibraryFolder + "\"");
            }
            CheckFolderAvailability(AppConfig.TemporaryFolder, true);
        }

        private bool CheckFolderAvailability(string folderName, bool shouldCreateFolder)
        {
            if (!Directory.Exists(folderName))
            {
                if (shouldCreateFolder)
                {
                    Directory.CreateDirectory(folderName);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
