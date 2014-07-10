using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OAuth2.Models;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class ReviewItemModel : BaseItemModel
    {
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Review; }
        }
        public BookItemModel Book { get; set; }
        public int ReviewRating { get; set; }
    }
}