using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.DataModel;

namespace Knigoskop.Site.Models
{
    public class MyBooksModel
    {
        public UserStatsModel Stats { get; set; }
        public IEnumerable<BaseItemModel> Books { get; set; }
        public int LastRowCount { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<GenreModel> Genres { get; set; }
    }
}