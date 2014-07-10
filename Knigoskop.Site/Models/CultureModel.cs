using System.Threading;
using Knigoskop.Site.Common.Mvc;

namespace Knigoskop.Site.Models
{
    public class CultureModel : UniqueIdentifierModel
    {
        public string Name { get; set; }
        public string Hint { get; set; }
        public string CultureName { get; set; }

        public bool IsCurrent
        {
            get { return Thread.CurrentThread.CurrentCulture.Name.Equals(CultureName); }
        }

        public bool IsDefault { get; set; }
        public string Subdomain { get; set; }
        public string Abc { get; set; }
    }
}