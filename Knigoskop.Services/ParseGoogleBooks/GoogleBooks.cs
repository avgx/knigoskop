using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Knigoskop.Services.ParseGoogleBooks
{
    public partial class ProcessBooks
    {
        private const string baseGoogleBooksUrl = @"https://www.googleapis.com/books/v1/volumes?q=";

        private GoogleBookJsonResponse GetBooksDataFromGoogle(string bookName, string author, int iterator)
        {
            string queryString = baseGoogleBooksUrl + " \"" + bookName + "\" " + author;

            KnigoskopWebClient webClient = new KnigoskopWebClient();
            try
            {
                string jsonGoogleBooksResponse = webClient.DownloadData(queryString);
                GoogleBookJsonResponse bookItem = GetGoogleBookItem(jsonGoogleBooksResponse);
                return bookItem;
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("(403)"))
                {
                    string currentTime = Convert.ToString(DateTime.Now);
                    Console.WriteLine(currentTime+" "+ex.Message);
                    Thread.Sleep((iterator + 1) * 5000);
                    if (iterator <= 10)
                    {
                        iterator++;
                        return GetBooksDataFromGoogle(bookName, author, iterator);
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private GoogleBookJsonResponse GetGoogleBookItem(string jsonGoogleBooksResponse)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GoogleBookJsonResponse));
            GoogleBookJsonResponse bookItem = null;
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonGoogleBooksResponse)))
            {
                bookItem = (GoogleBookJsonResponse)jsonSerializer.ReadObject(ms);
            }

            return bookItem;
        }
    }
}
