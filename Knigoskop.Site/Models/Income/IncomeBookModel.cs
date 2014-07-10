using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Knigoskop.Site.Models
{
    public class IncomeBookModel
    {
        public Guid? Id { get; set; }
        public Guid? ImageTempId { get; set; }
        public string Name { get; set; }
        public DateTime? Published { get; set; }
        public int? PagesCount { get; set; }
        public string Publisher { get; set; }
        public string Annotation { get; set; }
        public bool HasImage { get; set; }
        public byte[] Cover { get; set; }
        public string ISBN { get; set; }
    }
}