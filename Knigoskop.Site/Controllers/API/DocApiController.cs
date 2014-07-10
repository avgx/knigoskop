using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using Knigoskop.Site.Models.Api;
using ProtoBuf;

namespace Knigoskop.Site.Controllers.Api
{
    public class DocApiController : Controller
    {
        public ActionResult Index()
        {
            string s = Serializer.GetProto<SearchSuggestionsApiResponse>() + "\n";
            ViewBag.Message = s.Replace("package Knigoskop.Site.Models.Api;", "").Replace("fixed64", "double");
            IApiExplorer apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            return View(apiExplorer);
        }
    }
}