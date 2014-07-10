using System.Web;
using System.Web.Mvc;
using Knigoskop.Site.Common.Security;

namespace Knigoskop.Site.Common.Mvc
{
    public abstract class BaseWebViewPage<TModel> : WebViewPage<TModel>
    {
        public new SocialPrincipal User
        {
            get
            {
                var user = HttpContext.Current.User as SocialPrincipal;
                return user;
            }
        }
    }
}