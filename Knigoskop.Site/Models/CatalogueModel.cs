using System.Collections.Generic;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class CatalogueModel
    {
        public CatalogueViewTypeEnum ViewType { get; set; }
        public IEnumerable<BaseItemModel> Items { get; set; }
        public int FrontViewTotalCount { get; set; }
        public int BackViewTotalCount { get; set; }
        public int LastRowCount { get; set; }
        public ResultTypeEnum ResultType { get; set; }
        public string SelectedGenre { get; set; }
    }
}