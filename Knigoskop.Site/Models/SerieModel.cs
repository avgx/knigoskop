using System.Collections.Generic;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class SerieModel : BaseItemModel
    {        
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Serie; }
        }
        public IEnumerable<SerieBookModel> Books { get; set; }
    }
}