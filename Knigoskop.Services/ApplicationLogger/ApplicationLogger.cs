using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Knigoskop.Services.Logger
{
    public class ApplicationLogger
    {
        private const string LOG_FILE_NAME_EXT = ".log";
        private const string ERROR_FILE_NAME_EXT = ".err";

        private static string ApplicationName()
        {
            return Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private static string LogFileName()
        {
            return ApplicationName() + LOG_FILE_NAME_EXT;
        }

        private static string ErrorFileName()
        {
            return ApplicationName() + ERROR_FILE_NAME_EXT;
        }

        public static void WriteStringToLog(string logString)
        {
            WriteStringToFile(LogFileName(), GetTimestamp() + logString);
        }

        public static void WriteStringToError(string errorString)
        {
            WriteStringToFile(ErrorFileName(), GetTimestamp() + errorString);
        }

        private static string GetTimestamp()
        {
            return DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0') + " ";
        }

        public static void ClearLogFile()
        {
            ClearFile(LogFileName());
        }

        public static void ClearErrorFile()
        {
            ClearFile(ErrorFileName());
        }

        private static void ClearFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        private static void WriteStringToFile(string fileName, string line)
        {
            Console.WriteLine(line);
            using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            {
                using (TextWriter tw = new StreamWriter(fs))
                {
                    tw.WriteLine(line);
                    tw.Flush();
                }
            }
        }
    }
}
