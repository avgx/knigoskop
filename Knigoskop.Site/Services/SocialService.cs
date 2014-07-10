using System;
using System.Linq;
using System.Threading.Tasks;
using Facebook;
using Knigoskop.DataModel;
using Knigoskop.Site.Services.Interface;
using OAuth2;
using OAuth2.Client;

namespace Knigoskop.Site.Services
{
    public class SocialService : BasicDataService, ISocialService
    {
        private readonly IClient _client;
        private string _accessToken;

        private string BookStatToPostfix(UserStatStateEnum state)
        {
            if (state == UserStatStateEnum.WantsToRead)
                return "wants_to_read";
            return "reads";
        }

        private string GetSocialUserId(Guid userId)
        {
            using (Entities context = GetContext())
            {
                return
                    context.UserAuthProfiles
                        .Where(c => c.UserId == userId && c.ProviderName.ToLower() == _client.Name)
                        .Select(c => c.Id)
                        .FirstOrDefault();
            }
        }

        private string GetNormalizedUrl(string url)
        {
            return url.Replace("dev.knigoskop", "dev.www.knigoskop");
        }

        private string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    var client = new FacebookClient();

                    dynamic result = client.Get("oauth/access_token", new
                    {
                        client_id = _client.Configuration.ClientId,
                        client_secret = _client.Configuration.ClientSecret,
                        grant_type = "client_credentials"
                    });
                    _accessToken = result.access_token;
                }
                return _accessToken;
            }
        }

        public SocialService(AuthorizationRoot authorizationRoot)
        {
            _client = authorizationRoot.Clients.First(n => n is OAuth2.Client.Impl.FacebookClient);
        }

        public void RateBook(Guid userId, string bookUrl, short ratingValue, string reviewUrl = null, string actionId = null)
        {
            var socialUserId = GetSocialUserId(userId);
            if (string.IsNullOrEmpty(socialUserId))
                return;

            if (!string.IsNullOrEmpty(actionId))
                DeleteAction(actionId);

            Task.Factory.StartNew(() =>
            {
                var postParams = new JsonObject
                {
                    {"book", GetNormalizedUrl(bookUrl)},
                    {"rating:value", ratingValue},
                    {"rating:scale", 5}
                };

                if (!string.IsNullOrEmpty(reviewUrl) )
                    postParams.Add("review", GetNormalizedUrl(reviewUrl));

                var client = new FacebookClient(AccessToken);
                client.Post(string.Format("{0}/books.rates", socialUserId), postParams);
                
                SetBookStat(userId, bookUrl, UserStatStateEnum.Read);
            });
        }

        public void DeleteAction(string actionId)
        {
            if (!string.IsNullOrEmpty(actionId))
            {
                Task.Factory.StartNew(() =>
                    {
                        var client = new FacebookClient(AccessToken);
                        client.Delete(string.Format("{0}", actionId));
                    });
            }
        }

        public void SetBookStat(Guid userId, string bookUrl, UserStatStateEnum state, string actionId = null)
        {
            var socialUserId = GetSocialUserId(userId);
            if (string.IsNullOrEmpty(socialUserId) || state == UserStatStateEnum.None)
                return;

            if (!string.IsNullOrEmpty(actionId))
                DeleteAction(actionId);

            Task.Factory.StartNew(() =>
            {
                var postParams = new JsonObject
                {
                    {"book", GetNormalizedUrl(bookUrl)},
                    {"progress:timestamp", string.Format("{0:s}{0:zzz}", DateTime.Now)},
                    {"progress:percent_complete", state == UserStatStateEnum.IsReading ? 0.0 : 100}
                };
                
                var client = new FacebookClient(AccessToken);
                client.Post(string.Format("{0}/books.{1}", socialUserId, BookStatToPostfix(state)), postParams);
            });
        }
    }
}

