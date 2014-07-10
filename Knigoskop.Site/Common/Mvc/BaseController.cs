using System.Web.Mvc;
using Knigoskop.Site.Common.Security;

namespace Knigoskop.Site.Common.Mvc
{
    public class BaseController : Controller
    {
        public new SocialPrincipal User
        {
            get
            {
                var user = System.Web.HttpContext.Current.User as SocialPrincipal;
                return user;
            }
        }
    }
}