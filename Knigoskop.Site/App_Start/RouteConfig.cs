using System.Security.Policy;
using System.Web.Mvc;
using System.Web.Routing;
using Knigoskop.DataModel;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "robots.txt",
                url: "robots.txt",
                defaults: new { controller = "Home", action = "RobotsTxt" }
                );

            routes.MapRoute(
                name: "Auth",
                url: "account/auth",
                defaults: new { controller = "Account", action = "Auth" }
                );

            routes.MapRoute(
                name: "MyBooks",
                url: "account/books/{state}/{genreId}",
                defaults: new
                    {
                        controller = "Account",
                        action = "MyBooks",
                        genreId = UrlParameter.Optional
                    }
                );

            routes.MapRoute(
                name: "UserProfile",
                url: "account/user/{id}",
                defaults: new { controller = "Account", action = "UserProfile" }
            );

            routes.MapRoute(
                name: "MyThings",
                url: "account/mythings/{itemType}",
                defaults: new
                {
                    controller = "Account",
                    action = "MyThings",
                    itemType = ItemTypeEnum.Book.ToString().ToLower()
                });

            routes.MapRoute(
                name: "Citations",
                url: "account/citations",
                defaults: new
                {
                    controller = "Account",
                    action = "Citations"
                });

            routes.MapRoute(
                name: "Devices",
                url: "account/devices",
                defaults: new
                {
                    controller = "Account",
                    action = "Devices"
                });

            routes.MapRoute(
                name: "Settings",
                url: "account/settings",
                defaults: new { controller = "Account", action = "Settings" }
                );

            routes.MapRoute(
                name: "OpdsSettings",
                url: "account/opds",
                defaults: new { controller = "Account", action = "OpdsSettings" }
                );


            routes.MapRoute(
                name: "SignIn",
                url: "account/signin/{providerName}",
                defaults: new { controller = "Account", action = "SignIn" }
                );

            routes.MapRoute(
                name: "SignOut",
                url: "account/signout",
                defaults: new { controller = "Account", action = "SignOut" }
                );

            routes.MapRoute(
                name: "AdminIncomes",
                url: "admin/incomes/{viewType}",
                defaults: new { controller = "Admin", action = "Incomes", viewType = ItemTypeEnum.Book.ToString().ToLower() }
                );



            routes.MapRoute(
                name: "DocApi",
                url: "docapi",
                defaults: new { controller = "DocApi", action = "Index" }
                );

            routes.MapRoute(
                name: "Author",
                url: "author/{id}",
                defaults: new { controller = "Catalogue", action = "Author" }
                );

            routes.MapRoute(
                name: "DeliverBook",
                url: "deliver/{bookId}/{bookFormat}",
                defaults: new { controller = "Catalogue", action = "DeliverBook" }
            );


            routes.MapRoute(
                name: "Book",
                url: "book/{id}",
                defaults: new { controller = "Catalogue", action = "Book" }
                );


            routes.MapRoute(
                name: "BookBarCode",
                url: "isbn/{code}",
                defaults: new { controller = "Catalogue", action = "BarCodeImage" }
                );

            routes.MapRoute(
                 name: "AddBook",
                 url: "incomes/newbook",
                 defaults: new { controller = "Income", action = "AddBook" }
                 );


            routes.MapRoute(
                 name: "EditAuthor",
                 url: "incomes/author/{Id}",
                 defaults: new { controller = "Income", action = "EditAuthor" }
                 );

            routes.MapRoute(
                 name: "EditBook",
                 url: "incomes/book/{Id}",
                 defaults: new { controller = "Income", action = "EditBook" }
                 );


            routes.MapRoute(
                name: "EditSerie",
                url: "incomes/serie/{Id}",
                defaults: new { controller = "Income", action = "EditSerie" }
                );

            routes.MapRoute(
                 name: "AddReview",
                 url: "incomes/newreview/{bookId}",
                 defaults: new { controller = "Income", action = "AddReview", bookId = UrlParameter.Optional }
                 );

            routes.MapRoute(
                 name: "EditReview",
                 url: "incomes/review/{Id}",
                 defaults: new { controller = "Income", action = "EditReview", Id = UrlParameter.Optional }
                 );


            routes.MapRoute(
                name: "Serie",
                url: "serie/{id}",
                defaults: new { controller = "Catalogue", action = "Serie" }
                );

            routes.MapRoute(
                name: "Review",
                url: "review/{id}",
                defaults: new { controller = "Catalogue", action = "Review" }
                );

            routes.MapRoute(
                name: "AddComment",
                url: "methods/addcomment",
                defaults: new { controller = "Catalogue", action = "AddComment" }
                );


            routes.MapRoute(
                name: "SendComplain",
                url: "methods/complain",
                defaults: new { controller = "Catalogue", action = "SendComplain" }
                );

            routes.MapRoute(
                name: "RemoveComment",
                url: "methods/removecomment",
                defaults: new { controller = "Catalogue", action = "RemoveComment" }
                );

            routes.MapRoute(
                name: "SetRating",
                url: "methods/setrating",
                defaults: new { controller = "Catalogue", action = "SetRating" }
            );
            routes.MapRoute(
                name: "SetItemRating",
                url: "methods/setitemrating",
                defaults: new { controller = "Catalogue", action = "SetItemRating" }
            );
            routes.MapRoute(
                name: "SetUserStatToBook",
                url: "methods/setuserstat",
                defaults: new { controller = "Catalogue", action = "SetUserStatToBook" }
            );

            routes.MapRoute(
                name: "SetBookUserStat",
                url: "methods/getbookuserstat",
                defaults: new { controller = "Catalogue", action = "GetBookUserStat" }
            );

            routes.MapRoute(
                name: "CatalogueItems",
                url: "methods/items/{viewType}/{genreId}",
                defaults:
                    new
                        {
                            controller = "Catalogue",
                            action = "CatalogueItems",
                            viewType = CatalogueViewTypeEnum.Books.ToString().ToLower(),
                            genreId = UrlParameter.Optional
                        }
                );

            routes.MapRoute(
                name: "Catalogue",
                url: "catalogue/{viewType}/{genreId}",
                defaults:
                    new
                        {
                            controller = "Catalogue",
                            action = "Index",
                            viewType = CatalogueViewTypeEnum.Books.ToString().ToLower(),
                            genreId = UrlParameter.Optional
                        }
                );

            routes.MapRoute(
                name: "About",
                url: "about",
                defaults: new { controller = "Home", Action = "About" }
                );

            routes.MapRoute(
                name: "Disclaimer",
                url: "disclaimer",
                defaults: new { controller = "Home", Action = "Disclaimer" }
                );

            routes.MapRoute(
                name: "Contacts",
                url: "contacts",
                defaults: new { controller = "Home", Action = "Contacts" }
                );

            routes.MapRoute(
                name: "DefaultOpds",
                url: "opds/{action}/{id}",
                defaults: new { controller = "Opds", action = "Start", id = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );
        }
    }
}