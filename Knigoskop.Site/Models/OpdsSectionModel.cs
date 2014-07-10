using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Models
{
    public class OpdsSectionModel
    {
        public string BookName { get; set; }
        public IEnumerable<OpdsLinkModel> Items { get; set; }
    }
}