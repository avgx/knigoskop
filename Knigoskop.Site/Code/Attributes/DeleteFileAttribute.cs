using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Knigoskop.Site.Code.Attributes
{
    public class DeleteFileAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var result =  (filterContext.Result as FilePathResult);
            if (result != null)
            {
                filterContext.HttpContext.Response.Flush();
                if (File.Exists(result.FileName))
                    File.Delete(result.FileName);
            }            
        }
    }
}