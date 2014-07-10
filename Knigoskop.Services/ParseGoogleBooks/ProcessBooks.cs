using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Data.SqlClient;
using System.Threading;
using Knigoskop.DataModel;
using Knigoskop.Services.Logger;


namespace Knigoskop.Services.ParseGoogleBooks
{
    public partial class ProcessBooks
    {
        private string orderByClause;

        public ProcessBooks(string orderByClause)
        {
            if (!string.IsNullOrEmpty(orderByClause))
            {
                this.orderByClause = orderByClause;
            }
            GoThroughBookRecords();
        }

        private void GoThroughBookRecords()
        {
            using (Entities context = new Entities())
            {
                try
                {
                    IQueryable<Guid> books = null;
                    if (!string.IsNullOrEmpty(orderByClause))
                    {
                        books = context.Books.AsNoTracking().Where(x => x.FromGoogleBooks == null || x.FromGoogleBooks == false).OrderBy(x => x.Name).Select(x => x.BookId);
                    }
                    else
                    {
                        books = context.Books.AsNoTracking().Where(x => x.FromGoogleBooks == null || x.FromGoogleBooks == false).OrderBy(x => x.BookId).Select(x => x.BookId);
                    }
                    foreach (var bookID in books)
                    {
                        Book book = context.Books.FirstOrDefault(x => x.BookId == bookID);
                        ApplicationLogger.WriteStringToLog("Start to process book: " + book.BookId.ToString());
                        try
                        {
                            GoogleBookJsonResponse booksDataFromGoogle = GetBooksDataFromGoogle(book.Name, book.Authors.FirstOrDefault().Name, 0);
                            if (booksDataFromGoogle != null && booksDataFromGoogle.totalItems > 0)
                            {
                                ApplicationLogger.WriteStringToLog("Processed book: " + book.BookId.ToString() + " - Success");
                                BookItem bookItem = ChooseBookItem(booksDataFromGoogle, book.Name, book.Authors.FirstOrDefault().Name);
                                if (bookItem != null)
                                {
                                    UpdateBookRecord(context, book, bookItem);
                                    context.SaveChanges();
                                }
                                else
                                {
                                    UpdateBookRecordAsProcessed(context, book);
                                    context.SaveChanges();
                                }
                            }
                            else
                            {
                                UpdateBookRecordAsProcessed(context, book);
                                context.SaveChanges();
                            }
                        }
                        catch
                        {
                        }
                        Thread.Sleep(2000);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("provider: TCP Provider, error: 0"))
                    {
                        string currentTime = Convert.ToString(DateTime.Now);
                        ApplicationLogger.WriteStringToLog("Error: can't connect to SQL server.");
                        Thread.Sleep(5000);
                        GoThroughBookRecords();
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.WriteStringToLog(ex.Message);
                    throw ex;
                }
            }
        }
    }
}
