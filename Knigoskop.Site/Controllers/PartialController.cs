using System;
using System.Web.Mvc;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;
using OAuth2;

namespace Knigoskop.Site.Controllers
{
    public class PartialController : DataController
    {
        public PartialController(IDataService dataService, AuthorizationRoot authorizationRoot)
            : base(dataService, authorizationRoot)
        {
        }

        public PartialViewResult TopNavigation()
        {
            return PartialView();
        }

        public PartialViewResult Genres(Guid? genreId = null)
        {
            var model = DataService.GetGenres(genreId);
            return PartialView(model);
        }

        public PartialViewResult OpdsLinks(Guid bookId)
        {
            var model = DataService.GetOpdsSection(bookId, User.UserId);
            return PartialView(model);
        }

        public PartialViewResult DownloadLinks(Guid bookId)
        {
            var model = DataService.GetDownloadInfo(bookId, User.UserId);
            return PartialView(model);
        }
    }
}