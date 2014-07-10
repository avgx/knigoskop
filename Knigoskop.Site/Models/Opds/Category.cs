using System;
using System.Xml.Linq;

namespace Knigoskop.Site.Models.Opds
{
    public class Category : BaseItem
    {
        public string Scheme { get; set; }
        public string Term { get; set; }
        public string Label { get; set; }
        public override XElement Render()
        {
            throw new NotImplementedException();
        }
    }
}