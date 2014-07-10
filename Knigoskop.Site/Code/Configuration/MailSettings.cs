using System.Configuration;
using System.Web;
using Knigoskop.Site.Common.Helpers;

namespace Knigoskop.Site.Code.Configuration
{
    public class MailSettings : ConfigurationSection
    {
        private string GetFullEmail(object o)
        {
            if (((string) o).Contains("@"))
                return o as string;
            return string.Format("{0}@{1}", o, EmailDomain);
        }


        [ConfigurationProperty("emailDomain")]
        public string EmailDomain
        {
            get
            {
                string value = (string)this["emailDomain"];
                if (string.IsNullOrEmpty(value))
                    return HttpContext.Current.Request.Url.GetSiteDomain();
                return value;
            }

            set { this["emailDomain"] = value; }
        }


        [ConfigurationProperty("infoEmail", DefaultValue = "info")]
        public string InfoEmail
        {
            get
            {
                return GetFullEmail(this["infoEmail"]);
            }
            set
            {
                this["infoEmail"] = value;
            }
        }


        [ConfigurationProperty("supportEmail", DefaultValue = "support")]
        public string SupportEmail
        {
            get
            {
                return GetFullEmail(this["supportEmail"]);
            }
            set
            {
                this["supportEmail"] = value;
            }
        }

        [ConfigurationProperty("webmasterEmail", DefaultValue = "webmaster")]
        public string WebmasterEmail
        {
            get
            {
                return GetFullEmail(this["webmasterEmail"]);
            }
            set
            {
                this["webmasterEmail"] = value;
            }
        }

        [ConfigurationProperty("noreplyEmail", DefaultValue = "no-reply")]
        public string NoReplyEmail
        {
            get
            {
                return GetFullEmail(this["noreplyEmail"]);
            }
            set
            {
                this["noreplyEmail"] = value;
            }
        }

        [ConfigurationProperty("adminEmail", DefaultValue = "admin")]
        public string AdminEmail
        {
            get
            {
                return GetFullEmail(this["adminEmail"]);
            }
            set
            {
                this["adminEmail"] = value;
            }
        }

        [ConfigurationProperty("systemNotificationCultureName", DefaultValue = "ru-RU")]
        public string SystemNotificationCultureName
        {
            get
            {
                return (string)this["systemNotificationCultureName"];
            }
            set
            {
                this["systemNotificationCultureName"] = value;
            }
        }
    }
}