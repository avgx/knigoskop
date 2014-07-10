using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knigoskop.Services.Logger;

namespace Knigoskop.Services.UploadBooks
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ApplicationLogger.ClearLogFile();
                ApplicationLogger.ClearErrorFile();
                ApplicationLogger.WriteStringToLog("Application started.");

                LibraryData booksSources = new LibraryData();
                if (booksSources.NewBooksAreAvailable)
                {
                    LibraryUpdater booksUpload= new LibraryUpdater(booksSources.LastBookIdFromDatabase);
                }
                ApplicationLogger.WriteStringToLog("Application finished.");
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine("Application finished work with error. Please see error file.");
                ApplicationLogger.WriteStringToError(ex.Message);
                ApplicationLogger.WriteStringToLog("Application finished work with error. Please see error file.");
            }
        }
    }
}
