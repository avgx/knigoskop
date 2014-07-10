using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Models
{
    public class ViewStatsModel
    {
        public int Today { get; set; }
        public int Week { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Total { get; set; }
    }
}