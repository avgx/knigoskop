using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Knigoskop.Site.Models;
using Knigoskop.Site.Services.Interface;
using OAuth2;
using OAuth2.Configuration;

namespace Knigoskop.Site.Common.Mvc
{
    public class DataController : BaseController
    {
        protected IDataService DataService { get; private set; }
        protected AuthorizationRoot AuthorizationRoot { get; private set; }

        protected DataController(IDataService dataService, AuthorizationRoot authorizationRoot)
        {
            DataService = dataService;
            AuthorizationRoot = authorizationRoot;

            SetFacebOokkAplicationId();
        }
        
        private void  SetFacebOokkAplicationId ()
        {
            IClientConfiguration config =
                AuthorizationRoot.Clients.Where(c => c.Name == "Facebook").Select(c => c.Configuration).FirstOrDefault();
            if (config != null)
                ViewData["FacebookAppid"] = config.ClientId;
        }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {            
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            return base.BeginExecute(requestContext, callback, state);
        }

        protected void SetCookie(string key, string value, HttpResponseBase response)
        {
            var cookie = new HttpCookie(key, value) {Expires = DateTime.Now.AddYears(1)};
            if (response.Cookies[key] != null)
            {
                response.Cookies.Set(cookie);
            }
            else
            {
                response.Cookies.Add(cookie);
            }
        }
    }
}