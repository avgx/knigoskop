using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Models
{
    public class UserPanelModel
    {
        public int IncomesCount { get; set; }
        public  IEnumerable<string> Clients { get; set; }
    }
}