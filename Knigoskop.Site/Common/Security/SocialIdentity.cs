using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using FormsAuthenticationExtensions;
using Knigoskop.Site.Models;

namespace Knigoskop.Site.Common.Security
{
    [ComVisible(true)]
    [Serializable]
    public sealed class SocialIdentity : IIdentity
    {
        private readonly FormsAuthenticationTicket _ticket;

        public SocialIdentity(string name, FormsAuthenticationTicket ticket)
        {
            Name = name;
            _ticket = ticket;
        }

        public string FirstName
        {
            get { return GetValue("firstName"); }
        }

        public string LastName
        {
            get { return GetValue("lastName"); }
        }


        public string Email
        {
            get { return GetValue("email"); }
        }


        public DateTime AvatarUpdated
        {
            get { return new DateTime(Convert.ToInt64(GetValue("avatarUpdated"))); }
        }


        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        public string Name { get; private set; }


        public string AuthenticationType
        {
            get { return "SocialIdentity"; }
        }

        public bool IsAuthenticated
        {
            get { return !string.IsNullOrWhiteSpace(Name); }
        }

        private string GetValue(string valueName)
        {
            if (_ticket == null)
                return null;
            NameValueCollection values = HttpUtility.ParseQueryString(_ticket.UserData);
            return values[valueName];
        }


        public static void SetAuthTicket(IdentityInfoModel info, HttpResponseBase context)
        {
            var ticketData = new NameValueCollection
                {
                    {"firstName", info.FirstName},
                    {"lastName", info.LastName},
                    {"email", info.Email},
                    {"avatarUpdated", info.AvatarUpdated.Ticks.ToString()}
                };

            new FormsAuthentication().SetAuthCookie(context, info.Id.ToString(), true, ticketData);
        }

        public IdentityInfoModel ToIdentityInfo()
        {
            return new IdentityInfoModel
                {
                    Id = new Guid(Name),
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    AvatarUpdated = AvatarUpdated
                };
        }
    }
}