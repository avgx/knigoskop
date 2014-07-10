using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.DataModel;

namespace Knigoskop.Site.Models
{
    public class UserStatsModel
    {
        public int WantsToRead { get; set; }
        public int Read { get; set; }
        public int IsReading { get; set; }
        public UserStatStateEnum State { get; set; }
    }
}