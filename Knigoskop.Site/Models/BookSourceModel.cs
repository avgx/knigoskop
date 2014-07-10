using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Models
{
    public class BookSourceModel
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public byte[] Body { get; set; }
    }
}