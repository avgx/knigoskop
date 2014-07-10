using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knigoskop.Site.Common.Helpers
{
    public static class HttpHelper
    {
        public static bool IsFacebookAgent(this HttpRequestBase request)
        {
            return request.UserAgent != null && request.UserAgent.ToLowerInvariant().Contains("facebookexternalhit");
        }

        public static bool IsGoogleBot(this HttpRequestBase request)
        {
            return request.UserAgent != null && request.UserAgent.ToLowerInvariant().Contains("googlebot");
        }

        public static bool IsYandexBot(this HttpRequestBase request)
        {
            return request.UserAgent != null && request.UserAgent.ToLowerInvariant().Contains("yandexbot");
        }

        public static bool IsMsnBot(this HttpRequestBase request)
        {
            return request.UserAgent != null && request.UserAgent.ToLowerInvariant().Contains("msnbot");
        }

        public static bool IsBot(this HttpRequestBase request)
        {
            return request.IsFacebookAgent() || request.IsGoogleBot() || request.IsYandexBot() || request.IsMsnBot();
        }
    }
}
