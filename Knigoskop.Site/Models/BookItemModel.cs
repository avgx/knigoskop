using System.Collections.Generic;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class BookItemModel : BaseItemModel
    {
        public IEnumerable<BaseItemModel> Authors { get; set; }
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Book; }
        }

    }
}