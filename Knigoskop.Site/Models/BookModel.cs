using System;
using System.Collections.Generic;
using System.Web.UI.WebControls.Expressions;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class BookModel : BaseItemModel
    {
        public IEnumerable<BaseItemModel> Authors { get; set; }
        public IEnumerable<BaseItemModel> Translators { get; set; }
        public DateTime? Published { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public int? PagesCount { get; set; }
        public IEnumerable<GenreModel> Genres { get; set; }
        public IEnumerable<BaseItemModel> Series { get; set; }
        public IEnumerable<SerieBookModel> SerieBooks { get; set; }
        public IEnumerable<RelatedBookModel> RelatedBooks { get; set; }
        public IEnumerable<ReviewItemModel> Reviews { get; set; }
        public IEnumerable<CitationModel> Citations { get; set; }
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Book; }
        }
        public UserStatsModel UserStats { get; set; }
    }
}