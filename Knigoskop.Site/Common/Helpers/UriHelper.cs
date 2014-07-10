using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Common.Helpers
{
    public static class UriHelper
    {
        public static string RootUrl
        {
            get { return GetRootUrl(HttpContext.Current.Request.Url); }
        }

        public static string GetRootUrl(this Uri uri)
        {
            return uri.GetLeftPart(UriPartial.Authority);
        }

        public static string GetSiteDomain(this Uri uri)
        {
            return uri.Host
                      .Replace("dev.", string.Empty)
                      .Replace("www.", string.Empty);
        }

        public static string SetUrlParameter(this string url, string paramName, string value)
        {
            const string dummyHost = "http://www.dummy.com";
            return new Uri(dummyHost + url).SetParameter(paramName, value).ToString().Replace(dummyHost, string.Empty);
        }

        public static Uri SetParameter(this Uri url, string paramName, string value)
        {
            NameValueCollection queryParts = HttpUtility.ParseQueryString(url.Query);
            queryParts[paramName] = value;
            return new Uri(url.AbsoluteUriExcludingQuery() + '?' + queryParts);
        }

        public static string AbsoluteUriExcludingQuery(this Uri url)
        {
            return url.AbsoluteUri.Split('?').FirstOrDefault() ?? String.Empty;
        }
    }
}