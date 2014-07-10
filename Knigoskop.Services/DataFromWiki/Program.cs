using Knigoskop.DataModel;
using Knigoskop.Services;
using Knigoskop.Services.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Knigoskop.Services;

namespace What2ReadMe.AuthorsFromWiki
{
    partial class Program
    {
        static void Main(string[] args)
        {
            UploadDataFromWiki();
        }

        private static void UploadDataFromWiki()
        {
            using (Entities context = new Entities())
            {
                foreach (Author author in context.Authors.Where(x => !x.FromWiki))
                {
                    new AuthorsDataFromWiki(context, author);
                    context.SaveChanges();
                    ApplicationLogger.WriteStringToLog("Processed \"" + author.AuthorId.ToString() + "\": " + author.Name);
                }
            }
        }
    }
}
