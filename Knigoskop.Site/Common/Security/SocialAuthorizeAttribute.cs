using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Knigoskop.DataModel;

namespace Knigoskop.Site.Common.Security
{
    public class SocialAuthorizeAttribute : AuthorizeAttribute
    {
        private string _roleName;

        public SocialAuthorizeAttribute(string roleName = null)
        {
            _roleName = roleName;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorized = base.AuthorizeCore(httpContext);
            if (authorized)
            {
                if (!string.IsNullOrWhiteSpace(_roleName))
                    authorized = httpContext.User.IsInRole(_roleName);
            }
            return authorized;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                        {
                            {"lang", filterContext.RouteData.Values["lang"]},
                            {"controller", "account"},
                            {"action", "login"},
                            {"returnurl", filterContext.HttpContext.Request.RawUrl}
                        });
            }
        }
    }
}