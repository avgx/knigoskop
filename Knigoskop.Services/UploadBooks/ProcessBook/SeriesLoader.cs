using Knigoskop.DataModel;
using System;
using System.Linq;

namespace Knigoskop.Services.ProcessBook
{
    public class SeriesLoader
    {
        private InpBookRecord bookRecord;
        private Entities context;
        private Guid seriaId;
        private int seriesPosition;

        public int SeriesPosition
        {
            get { return seriesPosition; }
        }

        public Guid SeriaId
        {
            get { return seriaId; }
        }

        public SeriesLoader(InpBookRecord bookRecord, Entities context)
        {
            this.bookRecord = bookRecord;
            this.context = context;
            ProcesSeries();
        }

        private void ProcesSeries()
        {
            if (!string.IsNullOrEmpty(bookRecord.Seria) && bookRecord.SeriesPosition > 0)
            {
                seriesPosition = bookRecord.SeriesPosition;
                UploadSerieToDatabase(bookRecord.Seria, bookRecord.SeriesPosition);
            }
        }

        private void UploadSerieToDatabase(string serieName, int seriesPosition)
        {
            Serie serie = context.Series.FirstOrDefault(x => x.Name.ToLower() == serieName.ToLower());
            if (serie != null)
            {
                seriaId = serie.SerieId;
            }
            else
            {
                serie = new Serie();
                serie.SerieId = Guid.NewGuid();
                serie.Name = serieName;
                context.Series.Add(serie);
                seriaId = serie.SerieId;
            }

        }
    }
}
