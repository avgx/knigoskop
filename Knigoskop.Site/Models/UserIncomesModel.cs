using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.Expressions;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class UserIncomesModel 
    {

        public int BooksCount { get; set; }
        public int AuthorsCount { get; set; }
        public int SeriesCount { get; set; }
        public int ReviewsCount { get; set; }

        public IEnumerable<IncomeItemModel> Items
        {
            get; set;
        }

        public ItemTypeEnum ItemType { get; set; }
    }
}
