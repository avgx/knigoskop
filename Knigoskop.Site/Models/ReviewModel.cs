using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.Site.Models.Shared;
using OAuth2.Models;

namespace Knigoskop.Site.Models
{
    public class ReviewModel : BaseItemModel
    {        
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Review; }
        }
        public BookModel Book { get; set; }
        public int ReviewRating { get; set; }
    }
}