using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web.Mvc;
using BarcodeLib;
using Knigoskop.DataModel;
using Knigoskop.Site.Code.Attributes;
using Knigoskop.Site.Common.Helpers;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Common.Security;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services;
using Knigoskop.Site.Services.Interface;
using OAuth2;

namespace Knigoskop.Site.Controllers
{
    public class CatalogueController : DataController
    {
        private readonly ISocialService _socialService;
        private readonly IMailService _mailService;
        private readonly IConvertService _convertService;

        public CatalogueController(IDataService dataService, ISocialService socialService, IMailService mailService, IConvertService convertService, AuthorizationRoot authorizationRoot)
            : base(dataService, authorizationRoot)
        {
            _socialService = socialService;
            _mailService = mailService;
            _convertService = convertService;
        }

        [PageView(ItemTypeEnum.Author)]
        public ActionResult Author(Guid id)
        {
            AuthorModel model = DataService.GetAuthor(id, User.UserId);
            if (model == null)
                return new HttpNotFoundResult();
            return View(model);
        }

        [PageView(ItemTypeEnum.Book)]
        public ActionResult Book(Guid id)
        {
            BookModel model = DataService.GetBook(id, User.UserId);
            if (model == null)
                return new HttpNotFoundResult();
            return View(model);
        }

        [PageView(ItemTypeEnum.Serie)]
        public ActionResult Serie(Guid id)
        {
            SerieModel model = DataService.GetSerie(id, User.UserId);
            if (model == null)
                return new HttpNotFoundResult();
            return View(model);
        }


        [PageView(ItemTypeEnum.Review)]
        public ActionResult Review(Guid id)
        {
            ReviewModel model = DataService.GetReview(id, User.UserId);
            if (model == null)
                return new HttpNotFoundResult();
            return View(model);
        }

        [HttpPost]
        [SocialAuthorize]
        public PartialViewResult AddComment(Guid itemId, ItemTypeEnum itemType, Guid? parentCommentId, string commentText)
        {
            if (!string.IsNullOrEmpty(commentText) && User.UserId != null)
            {
                BaseItemModel data = null;
                Guid newItemId = DataService.AddComment(itemId, (Guid)User.UserId, parentCommentId, commentText, itemType);
                _mailService.NewComment(newItemId);
                switch (itemType)
                {
                    case ItemTypeEnum.Author:
                        data = DataService.GetAuthor(itemId, User.UserId);
                        break;
                    case ItemTypeEnum.Book:
                        data = DataService.GetBook(itemId, User.UserId);
                        break;
                    case ItemTypeEnum.Serie:
                        data = DataService.GetSerie(itemId, User.UserId);
                        break;
                    case ItemTypeEnum.Review:
                        data = DataService.GetReview(itemId, User.UserId);
                        break;
                }
                return PartialView("~/Views/Partial/Comments.cshtml", data);
            }
            return null;
        }

        [HttpPost]
        [SocialAuthorize]
        public void RemoveComment(Guid itemId, ItemTypeEnum itemType)
        {
            if (User.UserId != null)
            {
                DataService.DeleteComment(itemId);
            }
        }

        private CatalogueModel GetDataModel(CatalogueViewTypeEnum viewType, Guid? genreId, string searchQuery,
                                            int lastRowIndex = 0)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return DataService.GetCatalogueModel(viewType, genreId, lastRowIndex);
            return DataService.GetCatalogueModel(viewType, searchQuery, lastRowIndex);
        }

        public ActionResult Index(CatalogueViewTypeEnum viewType, Guid? genreId, string searchQuery,
                                  int lastRowIndex = 0)
        {
            CatalogueModel model = GetDataModel(viewType, genreId, searchQuery, lastRowIndex);
            if (!string.IsNullOrEmpty(searchQuery) && model.FrontViewTotalCount == 0 && model.BackViewTotalCount > 0)
            {
                return RedirectToAction("Index", "Catalogue", new { viewType = viewType == CatalogueViewTypeEnum.Books ? CatalogueViewTypeEnum.Authors.ToString().ToLower() : CatalogueViewTypeEnum.Books.ToString().ToLower(), genreId, searchQuery });
            }
            return View(model);
        }

        public PartialViewResult CatalogueItems(CatalogueViewTypeEnum viewType, Guid? genreId, string searchQuery,
                                                int lastRowIndex = 0)
        {
            CatalogueModel model = GetDataModel(viewType, genreId, searchQuery, lastRowIndex);
            return PartialView(model);
        }


        private BaseItemModel GetNewItemByType(ItemTypeEnum itemType)
        {
            BaseItemModel result = null;
            switch (itemType)
            {
                case ItemTypeEnum.Author:
                    result = new AuthorItemModel();
                    break;
                case ItemTypeEnum.Book:
                    result = new BookItemModel();
                    break;
                case ItemTypeEnum.Serie:
                    result = new SerieItemModel();
                    break;
                case ItemTypeEnum.Review:
                    result = new ReviewItemModel();
                    break;
            }
            return result;
        }

        private RatingModel DoSetRating(Guid itemId, Guid userId, ItemTypeEnum itemType, short ratingValue)
        {
            var result = DataService.SetRating(itemId, userId, ratingValue, itemType);
            if (itemType == ItemTypeEnum.Book)
            {
                DataService.SetUserStatToBook(itemId, (Guid)User.UserId, UserStatStateEnum.Read);
                _socialService.RateBook((Guid)User.UserId,
                                    Request.Url.GetRootUrl() + Url.Action("Book", "Catalogue", new { id = itemId }),
                                    ratingValue);
            }
            return result;
        }

        [HttpPost]
        [SocialAuthorize]
        public PartialViewResult SetRating(Guid itemId, ItemTypeEnum itemType, short ratingValue)
        {
            BaseItemModel model = GetNewItemByType(itemType);
            model.Id = itemId;
            model.Rating = DoSetRating(itemId, (Guid)User.UserId, itemType, ratingValue);
            return PartialView("~/Views/Partial/Rating.cshtml", model);
        }

        [HttpPost]
        [SocialAuthorize]
        public double SetItemRating(Guid itemId, ItemTypeEnum itemType, short ratingValue)
        {
            var result = DoSetRating(itemId, (Guid)User.UserId, itemType, ratingValue);
            return result.Value;
        }

        [HttpPost]
        [SocialAuthorize]
        public PartialViewResult SetUserStatToBook(Guid bookId, UserStatStateEnum state)
        {
            BookModel model = new BookModel
                {
                    Id = bookId,
                    UserStats = DataService.SetUserStatToBook(bookId, (Guid)User.UserId, state)
                };
            _socialService.SetBookStat((Guid)User.UserId,
                                       Request.Url.GetRootUrl() + Url.Action("Book", "Catalogue", new { id = bookId }),
                                       state);
            return PartialView("~/Views/Partial/UserStats.cshtml", model);
        }


        [HttpPost]
        [SocialAuthorize]
        public PartialViewResult GetBookUserStat(Guid bookId)
        {
            BookModel model = DataService.GetBook(bookId, User.UserId);
            return PartialView("~/Views/Partial/UserStats.cshtml", model);
        }

        [HttpPost]
        [SocialAuthorize]
        public void SendComplain(Guid itemId, string reasonText)
        {
            DataService.SendComplain(itemId, (Guid)User.UserId, reasonText);
            _mailService.ComplainEmail(itemId);
        }

        public FileResult BarCodeImage(string code)
        {
            using (var barCode = new Barcode())
            {
                barCode.IncludeLabel = true;
                barCode.LabelFont = new Font("Calibri", 11, FontStyle.Bold);
                barCode.LabelPosition = LabelPositions.BOTTOMCENTER;
                barCode.Encode(TYPE.ISBN, code, Color.Black, Color.White, 150, 80);
                return File(barCode.GetImageData(SaveTypes.PNG), "image/png");
            }
        }

        [SocialAuthorize]
        [DeleteFile]
        public ActionResult DeliverBook(Guid bookId, BookFormatEnum bookFormat, bool compress = true, string email = null)
        {
            var bookSource = DataService.GetBookSource(bookId, User.UserId, User.IsSuperUser);

            if (bookSource == null)
                return new HttpNotFoundResult();
            try
            {
                var target = _convertService.ConvertAndGetTargetFileName(bookSource, bookFormat, compress);

                
                if (!string.IsNullOrEmpty(email))
                {
                    DataService.AddDownload((Guid)User.UserId, bookId, DownloadTypeEnum.Email);
                    _mailService.DeliverBookToEmail(email, bookSource.Name, target);
                    if (System.IO.File.Exists(target.DownloadFileName))
                        System.IO.File.Delete(target.DownloadFileName);
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                
                DataService.AddDownload((Guid)User.UserId, bookId, DownloadTypeEnum.File);
                return new FilePathResult(target.FullFileName, System.Net.Mime.MediaTypeNames.Application.Octet)
                {
                    FileDownloadName = target.DownloadFileName
                };
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}