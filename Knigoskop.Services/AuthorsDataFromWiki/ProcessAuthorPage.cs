using HtmlAgilityPack;

namespace Knigoskop.Services
{
    public class ProcessAuthorPage
    {
        private AuthorData authorData;

        public AuthorData AuthorData
        {
            get { return authorData; }
        }

        public ProcessAuthorPage(string originalUrl, HtmlDocument htmlDocument)
        {
            FillAuthor(originalUrl, htmlDocument);
        }

        private void FillAuthor(string originalUrl, HtmlDocument htmlDocument)
        {
            if (htmlDocument != null)
            {
                HtmlNode vCard = htmlDocument.DocumentNode.SelectSingleNode("//table[@class=\"infobox vcard\"]");
                if (vCard == null)
                {
                    vCard = htmlDocument.DocumentNode.SelectSingleNode("//table[@class=\"infobox\"]");
                }
                authorData = new AuthorData(originalUrl, htmlDocument, vCard);
                if (IfAuthorDataDoesntContainData(authorData))
                {
                    authorData = null;
                }
            }
        }

        private bool IfAuthorDataDoesntContainData(AuthorData authorData)
        {
            if (authorData.AuthorImage == null && string.IsNullOrEmpty(authorData.Biography) && authorData.BornDate == null && authorData.DeathDate == null)
            {
                return true;
            }
            int linesCount = authorData.Biography.Split('\n').Length;
            if (linesCount > 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
