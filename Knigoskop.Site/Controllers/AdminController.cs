using System.Collections.Generic;
using System.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Knigoskop.Site.Code.Configuration;
using Knigoskop.Site.Common.Helpers;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Common.Security;
using Knigoskop.Site.Models;
using Knigoskop.Site.Services.Interface;
using OAuth2;
using Knigoskop.Site.Models.Shared;
using System;

namespace Knigoskop.Site.Controllers
{
    public class AdminController : DataController
    {
        private readonly IDataService _dataService;
        private readonly IMailService _mailService;
        private readonly ISocialService _socialService;
        public AdminController(IDataService dataService, AuthorizationRoot authorizationRoot, IMailService mailService, ISocialService socialService)
            : base(dataService, authorizationRoot)
        {
            _dataService = dataService;
            _mailService = mailService;
            _socialService = socialService;
        }

        [SocialAuthorize("Admin")]
        public ActionResult Incomes(ItemTypeEnum viewType)
        {
            var model = DataService.GetUnprocessedIncomes(viewType);
            return View(model);
        }

        [HttpPost]
        [SocialAuthorize]
        public void RemoveIncome(Guid id)
        {
            _dataService.DeleteIncomeItem(id);
        }

        [HttpPost]
        [SocialAuthorize("Admin")]
        public void ProcessIncome(Guid id)
        {
            ProceedItemModel newItem = _dataService.ProcessInсomeItem(id, (Guid)User.UserId);
            
            if (newItem != null && newItem.ItemType == ItemTypeEnum.Review)
            {
                _socialService.RateBook(
                    newItem.UserId,
                    Request.Url.GetRootUrl() + Url.Action("Book", "Catalogue", new { id = newItem.RefId }), 
                    newItem.Rating,
                    Request.Url.GetRootUrl() + Url.Action("Review", "Catalogue", new { id = newItem.Id }));
            }
            _mailService.IncomeApproved(id);
        }
    }
}