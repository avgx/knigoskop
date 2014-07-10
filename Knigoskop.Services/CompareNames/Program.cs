using Knigoskop.DataModel;
using Knigoskop.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareNames
{
    class Program
    {
        const string CONNECTION_NAME = "Knigoskop Database";
        static void Main(string[] args)
        {
            string updateSql = string.Empty;
            using (Entities context = new Entities())
            {
                foreach (Author author in context.Authors)
                {
                    string newAuthorName = ProcessAuthorName(author.Name);
                    if (!author.Name.Equals(newAuthorName))
                    {
                        updateSql += "Update Authors set Name='" + newAuthorName + "' where AuthorId='" +
                            author.AuthorId + "';\r\n";
                        Console.WriteLine(author.AuthorId + ":" + author.Name + "->" + newAuthorName);
                    }
                }
            }
            WriteStringToFile("UpdateAuthors.sql", updateSql);
        }

        private static void WriteStringToFile(string fileName, string text)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                TextWriter tw = new StreamWriter(fs);
                tw.Write(text);
                tw.Flush();
            }
        }

        private static string ProcessAuthorName(string authorName)
        {
            return TextNormalization.NormalizeAuthor(authorName);
        }

        private static string GetConnectionString()
        {
            string result = string.Empty;
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[CONNECTION_NAME];
            if (settings != null)
            {
                result = settings.ConnectionString;
            }
            return result;
        }
        private static void LevensteinTest()
        {
            LevensteinMetric test = new LevensteinMetric(100);
            int res = test.getDistance("результат", "подстава", 3);
        }
    }
}
