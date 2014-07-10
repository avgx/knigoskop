using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Knigoskop.Site.Common.Mvc;

namespace Knigoskop.Site.Models
{
    public class OpdsLinkModel : UniqueIdentifierModel
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string SearchUri { get; set; }
        public Guid? UserId { get; set; }
        public bool IsPersonalized
        {
            get
            {
                return UserId != null;
            }
        }
    }
}
