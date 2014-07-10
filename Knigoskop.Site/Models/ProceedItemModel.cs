using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class ProceedItemModel : UniqueIdentifierModel
    {
        public ItemTypeEnum ItemType { get; set; }
        public Guid UserId { get; set; }
        public Guid? RefId { get; set; }
        public short Rating { get; set; }
    }
}