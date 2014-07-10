using System.Web.Mvc;
using Knigoskop.Site.Code.Attributes;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Common.Security;

namespace Knigoskop.Site
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new PageViewAttribute());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SocialIdentityInjector());
        }
    }
}