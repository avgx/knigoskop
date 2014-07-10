
namespace Knigoskop.Services.ProcessBook
{
    public struct BookSources
    {
        private InpBookRecord bookRecord;
        private string bookFileName;

        public string BookFileName
        {
            get { return bookFileName; }
        }

        public InpBookRecord BookRecord
        {
            get { return bookRecord; }
        }

        public BookSources(InpBookRecord bookRecord, string bookFileName)
        {
            this.bookRecord = bookRecord;
            this.bookFileName = bookFileName;
        }
    }
}
