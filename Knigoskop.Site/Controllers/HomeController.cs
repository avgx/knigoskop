using System.Collections.Generic;
using System.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Knigoskop.Site.Code;
using Knigoskop.Site.Code.Configuration;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Models;
using Knigoskop.Site.Services.Interface;
using OAuth2;

namespace Knigoskop.Site.Controllers
{
    public class HomeController : DataController
    {
        public HomeController(IDataService dataService, AuthorizationRoot authorizationRoot)
            : base(dataService, authorizationRoot){}

        public ActionResult Index()
        {
            MainPageModel model = new MainPageModel
                {
                    Totals = DataService.GetWholeAmounts(),
                    TopBooks = DataService.GetTopBooks(),
                    TopReviews = DataService.GetTopReviews(0, 4)
                };            
            return View(model);
        }

        public ActionResult About()
        {
            var model = MailController.MailSettings;
            return View(model);
        }

        public ActionResult Disclaimer()
        {
            var model = MailController.MailSettings;
            return View(model);
        }

        public ActionResult RobotsTxt()
        {
            var config = (RobotsSettings)ConfigurationManager.GetSection("robotsConfiguration");
            Response.Clear();
            Response.ContentType = "text/plain";
            if (config.CrawlerAccess == CrawlerAccess.Public)
            {
                string configPath = HostingEnvironment.MapPath(config.RobotsConfigFile);
                if (configPath != null) Response.WriteFile(configPath);
            }
            else
            {
                const string response = "User-agent: *\nDisallow: /\n";
                Response.Write(response);
            }
            return null;
        }
    }
}