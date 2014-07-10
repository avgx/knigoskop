using Knigoskop.DataModel;
using System;
using System.Linq;

namespace Knigoskop.Services
{
    public class AuthorsDataFromWiki
    {
        public AuthorsDataFromWiki(Entities context, Author author)
        {
            if (AuthorNameIsValidated(author.Name))
            {
                AuthorPage authorPage = new AuthorPage(author.Name);
                ProcessAuthorPage processAuthorPage = new ProcessAuthorPage(authorPage.AuthorPageUrl, authorPage.AuthorPageHtmlDocument);
                if (processAuthorPage.AuthorData != null)
                {
                    UpdateAuthorResources(context, author, processAuthorPage.AuthorData);
                }
                else
                {
                    UpdateAuthorState(context, author);
                }
            }
            else
            {
                UpdateAuthorState(context, author);
            }
        }

        private void UpdateAuthorState(Entities context, Author author)
        {
            author.PhotoUpdated = DateTime.Now;
            author.FromWiki = true;
        }

        private void UpdateAuthorResources(Entities context, Author author, AuthorData authorData)
        {
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(authorData.Biography))
            {
                author.Biography = authorData.Biography;
                author.NameFromWiki = authorData.NameFromWiki;
                author.Photo = authorData.AuthorImage;
                author.PhotoUpdated = DateTime.Now;
                if (authorData.BornDate > new DateTime(1753, 1, 1))
                {
                    author.BirthDate = authorData.BornDate;
                }
                if (authorData.DeathDate > new DateTime(1753, 1, 1))
                {
                    author.DeathDate = authorData.DeathDate;
                }
                author.SourceUrl = authorData.OriginalUrl;
                author.FromWiki = true;
            }
        }

        private bool AuthorNameIsValidated(string authorName)
        {
            bool result = true;
            authorName = authorName.Replace('-', ' ');
            string acceptableChars = "АаБбВвГгДдЕеЁёЖжЗзИиЙйКкЛлМмНнОоПпРрСсТтУуФфХхЦцЧчШшЩщЪъЫыЬьЭэЮюЯя.'`  ()\u2019";
            for (int i = 0; i < authorName.Length; i++)
            {
                if (!acceptableChars.Contains(authorName[i]))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
    }
}
