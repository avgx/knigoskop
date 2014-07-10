using Knigoskop.Services.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knigoskop.Services.ParseGoogleBooks
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

                string orderByClause = string.Empty;
                if (args.Length > 0 && args[0].ToLower().StartsWith("order by "))
                {
                    orderByClause = args[0];
                }

                new ProcessBooks(orderByClause);

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
