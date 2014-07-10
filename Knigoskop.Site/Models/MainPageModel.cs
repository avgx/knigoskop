using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Knigoskop.Site.Models
{
    public class MainPageModel 
    {
        public WholeAmountModel Totals { get; set; }
        public IEnumerable<BookItemModel> TopBooks { get; set; }
        public IEnumerable<ReviewItemModel> TopReviews { get; set; }
    }
}
