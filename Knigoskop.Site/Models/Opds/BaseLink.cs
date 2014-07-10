using System.Xml.Linq;

namespace Knigoskop.Site.Models.Opds
{
    public abstract class BaseLink : BaseItem
    {
        public virtual string Rel { get; set; }
        public virtual string Href { get; set; }
        public abstract string Type { get; }
        public override XElement Render()
        {
            return new XElement("Link", AtomNsElement, new XAttribute("rel", Rel), new XAttribute("href", Href), new XAttribute("type", Type));
        }
    }
}