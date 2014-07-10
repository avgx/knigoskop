using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Knigoskop.Site.Services;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Common.Security
{
    public class SocialIdentityInjector : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            IIdentity identity = filterContext.HttpContext.User.Identity;

            HttpCookie formsCookie = filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            SocialIdentity newIdentity = formsCookie != null
                                             ? new SocialIdentity(identity.Name,
                                                                  FormsAuthentication.Decrypt(formsCookie.Value))
                                             : new SocialIdentity(identity.Name, null);

            var dataService = DependencyResolver.Current.GetService(typeof(IDataService)) as IDataService;

            var principal = new SocialPrincipal(newIdentity, dataService);

            filterContext.HttpContext.User = principal;
            Thread.CurrentPrincipal = principal;
        }
    }
}