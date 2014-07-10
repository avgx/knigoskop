using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web.Security;
using Knigoskop.DataModel;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Common.Security
{
    [ComVisible(true)]
    [Serializable]
    public sealed class SocialPrincipal : IPrincipal
    {
        private readonly SocialIdentity _identity;
        private readonly IDataService _dataService;
        private object _isSuperUser;

        public SocialPrincipal(SocialIdentity identity, IDataService dataService)
        {
            _identity = identity;
            _dataService = dataService;
        }

        public SocialIdentity Identity
        {
            get { return _identity; }
        }

        public Guid? UserId
        {
            get
            {
                if (_identity.IsAuthenticated)
                    return new Guid(_identity.Name);
                return null;
            }
        }

        public bool IsInRole(string role)
        {
            return _identity != null &&
                   !string.IsNullOrWhiteSpace(_identity.Name) && 
                   _identity.IsAuthenticated &&
                   !string.IsNullOrWhiteSpace(role) &&
                   _dataService.IsUserInRole( new Guid(_identity.Name), role);
        }

        IIdentity IPrincipal.Identity
        {
            get { return _identity; }
        }


        public bool IsSuperUser
        {
            get
            {
                if (_isSuperUser == null)
                    _isSuperUser = IsInRole("SUPERUSER");
                return (bool)_isSuperUser;

            }
        }
/*        public string GetAvatarUrl(HttpRequestBase request, long? key = null)
        {
            var urlHelper = new UrlHelper(request.RequestContext);
            return urlHelper.Action("avatar", "image", new { userId = (Guid)UserId, key = key ?? Identity.AvatarUpdated.Ticks });
        }
 */
    }
}