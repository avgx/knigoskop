using System.Xml.Linq;

namespace Knigoskop.Site.Models.Opds
{
    public class Author : BaseItem
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public override XElement Render()
        {
            return new XElement("Author", AtomNsElement, new XElement(AtomNs + "Name", Name), new XElement(AtomNs + "Uri", Uri));
        }
    }
}