using System.Globalization;
using System.Threading;

namespace Knigoskop.Site.Services
{
    public class BasicService
    {
        private CultureInfo _savedCultureName;

        public virtual string CurrentCultureName
        {
            get { return Thread.CurrentThread.CurrentCulture.Name; }
        }

        public virtual void SetCulture(string cultureName)
        {
            _savedCultureName = Thread.CurrentThread.CurrentCulture;
            var ci = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(ci.Name);
        }

        public virtual void RestoreCulture()
        {
            Thread.CurrentThread.CurrentCulture = _savedCultureName;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(_savedCultureName.Name);
        }
    }
}