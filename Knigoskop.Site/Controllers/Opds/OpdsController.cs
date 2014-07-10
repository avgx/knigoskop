using System.Web.Mvc;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Controllers.Opds
{
    public class OpdsController : Controller
    {
        private readonly IOpdsDataService _dataService;

        public OpdsController(IOpdsDataService dataService)
        {
            _dataService = dataService;
        }

        public ActionResult Start()
        {
            return View();
        }

    }
}
