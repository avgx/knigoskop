using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Knigoskop.DataModel;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Common.Security;
using Knigoskop.Site.Models;
using Knigoskop.Site.Services.Interface;
using OAuth2;
using OAuth2.Client;
using OAuth2.Configuration;
using OAuth2.Models;
using Knigoskop.Site.Models.Shared;
using System.Web;
using System.IO;
using Knigoskop.Site.Localization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml.Linq;

namespace Knigoskop.Site.Controllers
{
    public class AccountController : DataController
    {
        private const string LoginStoreKey = "_loginStore";
        private IClient _currentClient;

        public AccountController(IDataService dataService, AuthorizationRoot authorizationRoot)
            : base(dataService, authorizationRoot) { }


        private LoginStore Store
        {
            get
            {
                object o = Session[LoginStoreKey];
                if (o == null)
                {
                    o = new LoginStore();
                    Session[LoginStoreKey] = o;
                }
                return (LoginStore)o;
            }
        }

        public PartialViewResult UserPanel()
        {
            Store.SourceUrl = Request.RawUrl;
            var model = new UserPanelModel
            {
                Clients = AuthorizationRoot.Clients.Select(x => x.Name)
            };
            if (User.UserId != null && User.IsInRole("ADMIN"))
                model.IncomesCount = DataService.GetUnprocessedIncomesCount();
            return PartialView(model);
        }

        private RedirectResult GetRedirectToSourceUrl()
        {
            string redirectUrl = Store.SourceUrl;
            if (string.IsNullOrEmpty(redirectUrl))
                redirectUrl = "/";
            return new RedirectResult(redirectUrl);
        }

        private IClient CurrentClient
        {

            get
            {
                return _currentClient ?? (_currentClient = AuthorizationRoot.Clients
                                                                             .First(
                                                                                 x =>
                                                                                 x.Name.Equals(
                                                                                     Store.ProviderName,
                                                                                     StringComparison
                                                                                         .InvariantCultureIgnoreCase)));
            }
        }


        public RedirectResult Auth(string code, string error)
        {
            UserInfo userInfo = CurrentClient.GetUserInfo(Request.QueryString);
            IdentityInfoModel info = DataService.AuthorizeUser(userInfo, User.UserId);
            if (!User.Identity.IsAuthenticated)
                SocialIdentity.SetAuthTicket(info, Response);
            return GetRedirectToSourceUrl();
        }

        public RedirectResult SignIn(string providerName)
        {
            Store.ProviderName = providerName;
            return new RedirectResult(CurrentClient.GetLoginLinkUri());
        }

        public RedirectResult SignOut()
        {
            FormsAuthentication.SignOut();
            return GetRedirectToSourceUrl();
        }

        private class LoginStore
        {
            public string SourceUrl { get; set; }
            public string ProviderName { get; set; }
        }

        [SocialAuthorize]
        public ActionResult MyBooks(UserStatStateEnum state, int lastRowIndex = 0, Guid? genreId = null)
        {
            MyBooksModel model = DataService.GetMyBooks((Guid)User.UserId, state, lastRowIndex, genreId);
            return View(model);
        }

        public PartialViewResult MyBooksItems(UserStatStateEnum state, int lastRowIndex = 0, Guid? genreId = null)
        {
            MyBooksModel model = DataService.GetMyBooks((Guid)User.UserId, state, lastRowIndex, genreId);
            return PartialView(model);
        }


        [SocialAuthorize]
        public ActionResult Settings()
        {
            UserProfileModel model = new UserProfileModel
                {
                    Identity = DataService.GetUserProfile((Guid)User.UserId),
                    AvailableProviders = AuthorizationRoot.Clients.ToList()
                };
            return View(model);
        }

        [SocialAuthorize]
        [HttpPost]
        public ActionResult Settings(UserProfileModel model)
        {
            model.Identity.Id = (Guid)User.UserId;
            model.AvailableProviders = AuthorizationRoot.Clients.ToList();
            DataService.UpdateUserProfile(model.Identity);
            if (Session["TempAvatar"] != null)
            {
                DataService.UpdateUserAvatar((Guid)User.UserId, Session["TempAvatar"] as byte[]);
                Session.Remove("TempAvatar");
            }
            SocialIdentity.SetAuthTicket(model.Identity, Response);
            return View(model);
        }


        [HttpPost]
        public void UploadAvatar(HttpPostedFileBase image)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream = new MemoryStream();
            image.InputStream.CopyTo(memoryStream);
            var data = memoryStream.ToArray();
            Session["TempAvatar"] = data;
        }

        public FileResult TempAvatar()
        {
            byte[] fileContent = Session["TempAvatar"] as byte[];
            return File(fileContent, "image/png");
        }

        [SocialAuthorize]
        public ActionResult MyThings(ItemTypeEnum itemType)
        {
            UserIncomesModel model = DataService.GetUserIncomes((Guid)User.UserId, itemType);
            return View(model);
        }

        public ActionResult UserProfile(Guid id)
        {
            var model = DataService.GetUserProfile(id);
            //MyThingsStatsModel model = DataService.GetMyThingsStats((Guid)User.UserId, itemType);
            return View(model);
        }

        [SocialAuthorize]
        public ActionResult OpdsSettings()
        {
            return View();
        }
        public JsonResult LoadOpds()
        {
            var links = DataService.GetOpdsLinks((Guid)User.UserId);
            return Json(links, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveOpdsSource(Guid? Id, string Name, string Url)
        {
            var searchUrl = GetOpdsSearchUrl(Url);
            if (string.IsNullOrEmpty(searchUrl))
            {
                return Json(new { success = false, data = Text.InvalidOPDSUrlMessage });
            }

            if (Id == null)
            {
                Id = DataService.AddOpdLink(Name, Url, searchUrl, (Guid)User.UserId);

            }
            else
            {
                DataService.UpdateOpdLink(new OpdsLinkModel() { Id = (Guid)Id, Uri = Url, Name = Name, UserId = (Guid)User.UserId, SearchUri = searchUrl });

            }
            return Json(new { success = true, data = Id });


        }
        public JsonResult RemoveOpdsSource(Guid Id)
        {
            DataService.DeleteOpdsLink(Id);
            return Json(string.Empty);
        }

        private string GetOpdsSearchUrl(string url)
        {
            try
            {
                XDocument doc = XDocument.Load(url);
                var link = doc.Root.Elements().FirstOrDefault(el => el.Name.LocalName == "link" && el.Attribute("rel").Value == "search" && el.Attribute("type").Value == "application/atom+xml");
                if (link == null)
                    return null;
                string result = link.Attribute("href").Value;
                if (!result.StartsWith("http"))
                {
                    Uri uri = new Uri(url);
                    var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port);
                    result = string.Format("{0}{1}", builder.Uri, result.TrimStart('/'));
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [SocialAuthorize]
        public ActionResult Devices()
        {
            return View();
        }
        [SocialAuthorize]
        public JsonResult LoaDevices()
        {
            var links = DataService.GetDevices((Guid)User.UserId);
            return Json(links, JsonRequestBehavior.AllowGet);
        }
        [SocialAuthorize]
        public void DeleteDevice(Guid id)
        {
            DataService.DeleteDevice(id);

        }
        [SocialAuthorize]
        public Guid AddDevice(string name, string email)
        {
            return DataService.AddDevice(name, email, (Guid)User.UserId);
        }
        [SocialAuthorize]
        public void UpdateDevice(DeviceModel device)
        {
            device.UserId = (Guid)User.UserId;
            DataService.UpdateDevice(device);
        }

        [SocialAuthorize]
        public ActionResult Citations()
        {
            var model = DataService.GetUserCitations((Guid) User.UserId);
            return View(model);
        }
    }
}