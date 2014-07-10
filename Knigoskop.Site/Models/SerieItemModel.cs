using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class SerieItemModel : BaseItemModel
    {
        public IEnumerable<BaseItemModel> Authors { get; set; }
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Serie; }
        }

    }
}