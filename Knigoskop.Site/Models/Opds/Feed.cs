using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Knigoskop.Site.Models.Opds
{
    public class Feed : BaseFeedItem
    {
        public override DocumentTypeEnum Type
        {
            get { return DocumentTypeEnum.Feed; }
        }
        public IEnumerable<Entry> Entries { get; set; }

        public XDocument GetXml()
        {
            return new XDocument(new XDeclaration("1.0", "UTF-8", ""));
        }

        public override XElement Render()
        {
            throw new NotImplementedException();
        }
    }
}