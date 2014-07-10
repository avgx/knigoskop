using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Knigoskop.Site.Common.Helpers
{
    public static class MvcHelper
    {
        public static void RenderPartialWithData(this HtmlHelper htmlHelper, string partialViewName, object model, object viewData)
        {
            var viewDataDictionary = new ViewDataDictionary();
            if (viewData != null)
            {
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(viewData))
                {
                    object val = prop.GetValue(viewData);
                    viewDataDictionary[prop.Name] = val;
                }
            }
            htmlHelper.RenderPartial(partialViewName, model, viewDataDictionary);
        }
    }
}