using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Knigoskop.Site.Models.Opds
{
    public class Entry : BaseFeedItem
    {
        public override DocumentTypeEnum Type
        {
            get { return DocumentTypeEnum.Entry; }
        }

        public IEnumerable<Category> Categories { get; set; }
        public string Summary { get; set; }
        public string Rights { get; set; }
        public string Isbn { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }
        public DateTime Issued { get; set; }

        public override XElement Render()
        {
            throw new NotImplementedException();
        }
    }
}