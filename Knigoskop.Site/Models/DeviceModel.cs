using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Knigoskop.Site.Common.Mvc;

namespace Knigoskop.Site.Models
{
    public class DeviceModel : UniqueIdentifierModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Guid UserId { get; set; }
    }
}
