using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Knigoskop.Services
{
    public class AppConfig
    {
        const string CONNECTION_NAME = "Knigoskop Database";
        const string LIBRARY_FOLDER = "Library Folder";
        const string TEMPORARY_FOLDER = "Temporary Folder";
        const string BOOKS_FOLDER = "Books Folder";
        const string CONNECTION_STRING_NAME = "Knigoskop Database";

        public static string LibraryFolder
        {
            get
            {
                string result = ConfigurationManager.AppSettings[LIBRARY_FOLDER];
                if (!result.EndsWith("\\"))
                {
                    result = result + "\\";
                }
                return result;
            }
        }

        public static string TemporaryFolder
        {
            get
            {
                string result = ConfigurationManager.AppSettings[TEMPORARY_FOLDER];
                if (!result.EndsWith("\\"))
                {
                    result = result + "\\";
                }
                return result;
            }
        }

        public static string BooksFolder
        {
            get
            {
                string result = ConfigurationManager.AppSettings[BOOKS_FOLDER];
                if (!result.EndsWith("\\"))
                {
                    result = result + "\\";
                }
                return result;
            }
        }
    }
}
