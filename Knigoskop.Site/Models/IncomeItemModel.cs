using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class IncomeItemModel : UniqueIdentifierModel
    {
        public ItemTypeEnum ItemType { get; set; }
        public string Name { get; set; }
        public IncomeStateEnum State { get; set; }
        public DateTime Created { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsPublished { get; set; }
    }
}
