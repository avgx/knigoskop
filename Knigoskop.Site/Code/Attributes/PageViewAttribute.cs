using System;
using System.Web.Mvc;
using Knigoskop.Site.Common.Helpers;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Common.Security;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Code.Attributes
{
    public class PageViewAttribute : ActionFilterAttribute
    {
        private readonly ItemTypeEnum _itemType;
        private readonly string _foreighIdName;

        public PageViewAttribute(ItemTypeEnum itemType = ItemTypeEnum.Book, string foreighIdName = "id")
        {
            _itemType = itemType;
            _foreighIdName = foreighIdName;
        }

        public IDataService DataService { get; set; }

        private const string KEY = "sessionKey";

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);

            if (DataService != null && context.HttpContext.Session != null && !context.HttpContext.Request.IsBot() && !((SocialPrincipal)context.HttpContext.User).IsSuperUser)
            {
                var controller = context.Controller as BaseController;
                if (controller != null)
                {
                    var sessionId = context.HttpContext.Session[KEY] as string;
                    var foreignId = context.RequestContext.RouteData.Values[_foreighIdName] as string;
                    if (string.IsNullOrEmpty(sessionId))
                    {
                        sessionId = Guid.NewGuid().ToString();
                        context.HttpContext.Session[KEY] = sessionId;
                    }
                    if (!string.IsNullOrEmpty(foreignId))
                    {
                        var request = context.HttpContext.Request;
                        DataService.SetPageView(new Guid(sessionId), _itemType, request.UserAgent, request.UserHostAddress, new Guid(foreignId), controller.User.UserId);
                    }
                }
            }
        }

    }
}
