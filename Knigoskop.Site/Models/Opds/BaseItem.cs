using System.Xml.Linq;

namespace Knigoskop.Site.Models.Opds
{
    public abstract class BaseItem
    {
        protected readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom/";
        protected readonly XNamespace DcNs = "http://purl.org/dc/terms/";
        protected readonly XNamespace OpdsNs = "http://opds-spec.org/2010/catalog/";

        protected XAttribute AtomNsElement
        {
            get
            {
                return new XAttribute(XNamespace.Xmlns + "atom", AtomNs);
            }
        }

        protected XAttribute DcNsElement
        {
            get
            {
                return new XAttribute(XNamespace.Xmlns + "dc", DcNs);
            }
        }

        protected XAttribute OpdsNsElement
        {
            get
            {
                return new XAttribute(XNamespace.Xmlns + "opds", OpdsNs);
            }
        }

        public abstract XElement Render();
    }
}