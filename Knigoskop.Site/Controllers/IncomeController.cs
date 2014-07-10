using System;
using System.Web.Mvc;
using Knigoskop.DataModel;
using Knigoskop.Site.Code.Attributes;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Common.Security;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;
using OAuth2;
using System.Web;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
namespace Knigoskop.Site.Controllers
{
    [SocialAuthorize]
    public class IncomeController : DataController
    {
        private readonly IMailService _mailService;

        public IncomeController(IDataService dataService, AuthorizationRoot authorizationRoot, IMailService mailService)
            : base(dataService, authorizationRoot)
        {
            _mailService = mailService;
        }

        public ActionResult AddBook()
        {
            @ViewBag.BookId = "";
            return View();

        }
        public ActionResult EditBook(Guid? Id, bool isIncome = false)
        {
            @ViewBag.BookId = Id.ToString();
            @ViewBag.IsIncome = isIncome;
            return View("AddBook");

        }
        public ActionResult EditAuthor(Guid? Id, bool isIncome = false)
        {
            @ViewBag.AuthorId = Id.ToString();
            @ViewBag.IsIncome = isIncome;
            return View();
        }
        public ActionResult EditSerie(Guid? Id, bool isIncome = false)
        {
            @ViewBag.SerieId = Id.ToString();
            @ViewBag.IsIncome = isIncome;
            return View();

        }
        public ActionResult AddReview(Guid? bookId)
        {
            @ViewBag.BookId = bookId;
            return View();

        }

        public ActionResult EditReview(Guid? Id, bool isIncome = false)
        {
            @ViewBag.IncomeId = Id.ToString();
            @ViewBag.IsIncome = isIncome;
            return View("AddReview");
        }

        public JsonResult LoadSerie(Guid serieId)
        {
            IncomeSerieModel model;
            Guid? userId = null;
            if (!User.IsInRole("ADMIN"))
                userId = User.UserId;
            var serieXml = DataService.GetIncomeItem(ItemTypeEnum.Serie, serieId, userId);
            if (!string.IsNullOrEmpty(serieXml))
            {
                model = DeserializeModel<IncomeSerieModel>(serieXml);
            }
            else
            {
                var serie = DataService.GetSerie(serieId, null);
                model = new IncomeSerieModel
                {
                    Id = serie.Id,
                    Name = serie.Name,
                    Description = serie.Description
                };
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadAuthor(Guid authorId)
        {
            IncomeAuthorModel model;
            Guid? userId = null;
            if (!User.IsInRole("ADMIN"))
                userId = User.UserId;
            var authorXml = DataService.GetIncomeItem(ItemTypeEnum.Author, authorId, userId);
            if (!string.IsNullOrEmpty(authorXml))
            {
                model = DeserializeModel<IncomeAuthorModel>(authorXml);
                if (model.Photo != null)
                {
                    model.ImageTempId = Guid.NewGuid();
                    Session[model.ImageTempId.ToString()] = model.Photo;
                    model.Photo = null;
                }
            }
            else
            {
                var author = DataService.GetAuthor(authorId, null);
                model = new IncomeAuthorModel
               {
                   Biography = author.Description,
                   BirthDate = author.BornDate,
                   DeathDate = author.DeathDate,
                   Id = author.Id,
                   Name = author.Name,
                   HasImage = author.HasImage
               };
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadGenres()
        {
            var genres = DataService.GetGenres(null);
            return Json(genres, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadBook(Guid bookId)
        {
            IncomeModel model;
            Guid? userId = null;
            if (!User.IsInRole("ADMIN"))
                userId = User.UserId;
            var bookXml = DataService.GetIncomeItem(ItemTypeEnum.Book, bookId, userId);

            if (!string.IsNullOrEmpty(bookXml))
            {
                model = DeserializeModel<IncomeModel>(bookXml);
                if (model.IncomeBook.Cover != null)
                {
                    model.IncomeBook.ImageTempId = Guid.NewGuid();
                    Session[model.IncomeBook.ImageTempId.ToString()] = model.IncomeBook.Cover;
                    model.IncomeBook.Cover = null;
                }
                foreach (var author in model.IncomeAuthors)
                {
                    if (author.Photo != null)
                    {
                        author.ImageTempId = Guid.NewGuid();
                        Session[author.ImageTempId.ToString()] = author.Photo;
                        author.Photo = null;
                    }
                }
            }
            else
            {
                var book = DataService.GetBook(bookId, null);
                model = new IncomeModel()
                {
                    Id = book.Id,
                    IncomeBook = new IncomeBookModel()
                    {
                        Id = book.Id,
                        Annotation = book.Description,
                        Name = book.Name,
                        Published = book.Published,
                        Publisher = book.Publisher,
                        PagesCount = book.PagesCount,
                        HasImage = book.HasImage,
                        ISBN = book.ISBN

                    },
                    IncomeAuthors = book.Authors.Select(s =>
                    {
                        var author = DataService.GetAuthor(s.Id, null);
                        return new IncomeAuthorModel
                        {
                            Biography = author.Description,
                            BirthDate = author.BornDate,
                            DeathDate = author.DeathDate,
                            Id = author.Id,
                            Name = author.Name,
                            HasImage = author.HasImage
                        };
                    }).ToList(),
                    IncomeSeries = book.Series.Select(s => new IncomeSerieModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description
                    }).ToList(),
                    IncomeGenres = book.Genres.Select(s => new IncomeGenreModel()
                    {
                        Id = s.Id,
                        Name = s.Name
                    }).ToList(),
                    IncomeSimilarBooks = book.RelatedBooks.Select(s => new IncomeSimilarBookModel()
                    {
                        Id = s.Id,
                        Name = s.Name
                    }).ToList()

                };
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadReview(Guid incomeId)
        {
            IncomeReviewModel model = null;
            var reviewXml = DataService.GetIncomeItem(ItemTypeEnum.Review, incomeId);
            if (!string.IsNullOrEmpty(reviewXml))
            {
                model = DeserializeModel<IncomeReviewModel>(reviewXml);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public bool SaveReview(IncomeReviewModel model, bool isIncome = false)
        {
            try
            {
                var xmlString = SerializeModel(model);
                if (model.Id != null)
                {
                    DataService.UpdateIncomeItem((Guid)model.Id, model.Name, xmlString);
                }
                else
                {
                    DataService.ApplyIncomeItem(ItemTypeEnum.Review, (Guid)User.UserId, model.Name, xmlString, model.Id);
                }
                return true;

            }
            catch (Exception ex)
            {
                // todo
            }
            return false;
        }
        [HttpPost]
        public bool SaveAuthor(IncomeAuthorModel model, bool isIncome = false)
        {
            try
            {
                if (model.ImageTempId != null)
                    model.Photo = GetFileContent(model.ImageTempId);
                var xmlString = SerializeModel(model);
                if (User.IsInRole("ADMIN") && isIncome && model.Id != null)
                {
                    DataService.UpdateIncomeItem((Guid)model.Id, model.Name, xmlString);
                }
                else
                {
                    var id = DataService.ApplyIncomeItem(ItemTypeEnum.Author, (Guid)User.UserId, model.Name, xmlString, model.Id);
                    _mailService.NewIncome(id);
                }

                return true;

            }
            catch (Exception ex)
            {
                // todo
            }
            return false;
        }
        [HttpPost]
        public bool SaveSerie(IncomeSerieModel model, bool isIncome = false)
        {
            try
            {
                var xmlString = SerializeModel(model);
                if (User.IsInRole("ADMIN") && isIncome && model.Id != null)
                {
                    DataService.UpdateIncomeItem((Guid)model.Id, model.Name, xmlString);
                }
                else
                {
                    var id = DataService.ApplyIncomeItem(ItemTypeEnum.Serie, (Guid)User.UserId, model.Name, xmlString, model.Id);
                    _mailService.NewIncome(id);
                }
                return true;
            }
            catch (Exception ex)
            {
                // todo
            }
            return false;
        }
        [HttpPost]
        public bool SaveBook(IncomeModel model, bool isIncome = false)
        {
            try
            {
                if (model.IncomeBook.ImageTempId != null)
                    model.IncomeBook.Cover = GetFileContent(model.IncomeBook.ImageTempId);
                foreach (var author in model.IncomeAuthors)
                {
                    if (author.ImageTempId != null)
                        author.Photo = GetFileContent(author.ImageTempId);
                }
                var xmlString = SerializeModel(model);
                if (User.IsInRole("ADMIN") && isIncome && model.Id != null)
                {
                    DataService.UpdateIncomeItem((Guid)model.Id, model.IncomeBook.Name, xmlString);
                }
                else
                {
                    var id = DataService.ApplyIncomeItem(ItemTypeEnum.Book, (Guid)User.UserId, model.IncomeBook.Name, xmlString, model.Id);
                    _mailService.NewIncome(id);

                }
                return true;
            }
            catch (Exception ex)
            {
                // todo
            }
            return false;
        }


        public FileResult Image(Guid? imageId)
        {
            byte[] fileContent = GetFileContent(imageId);

            return File(fileContent, "image/png");
        }
        [HttpPost]
        public string UploadImage(HttpPostedFileBase image)
        {
            MemoryStream memoryStream = new MemoryStream();
            memoryStream = new MemoryStream();
            image.InputStream.CopyTo(memoryStream);
            var data = memoryStream.ToArray();
            var imageId = Guid.NewGuid();
            Session[imageId.ToString()] = data;
            return imageId.ToString();
        }
        private byte[] GetFileContent(Guid? imageId)
        {
            if (imageId == null || Session[imageId.ToString()] == null)
            {
                return null;
            }
            else
            {
                return (byte[])Session[imageId.ToString()];
            }
        }
        private string SerializeModel(Object model)
        {
            StringBuilder builder = new StringBuilder();
            XmlSerializer s = new XmlSerializer(model.GetType());
            using (StringWriter writer = new StringWriter(builder))
            {
                s.Serialize(writer, model);
            }
            return builder.ToString();
        }
        private T DeserializeModel<T>(string modelXml)
        {
            StringBuilder builder = new StringBuilder();
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(modelXml))
            {
                return (T)s.Deserialize(reader);
            }
        }
    }
}
