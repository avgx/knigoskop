using System.Collections.Generic;
using OAuth2.Client;

namespace Knigoskop.Site.Models
{
    public class UserProfileModel
    {
        public IdentityInfoModel Identity { get; set; }
        public IList<IClient> AvailableProviders { get; set; }
    }
}