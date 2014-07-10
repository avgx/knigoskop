using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Knigoskop.Services
{
    public enum TypeOfWikiPage
    {
        ListOfNamesakes,
        AlternativesList,
        HitList,
        NothingFound,
        Undefined
    }
    public class AuthorPage
    {
        const string WIKI_SEARCH_URL = "http://ru.wikipedia.org/w/index.php?title=Служебная%3AПоиск&profile=default&search={0}&fulltext=Search";
        const string WIKI_BASE_PAGE = "http://ru.wikipedia.org";
        const string CREATE_PAGE_STR = "Создать страницу";
        const string PAGE_IS_EXIST = "Есть страница";

        private HtmlDocument authorPageHtmlDocument;
        private string authorPageUrl;
        private string authorName;

        public string AuthorPageUrl
        {
            get { return authorPageUrl; }
        }

        public HtmlDocument AuthorPageHtmlDocument
        {
            get { return authorPageHtmlDocument; }
        }

        public AuthorPage(string authorName)
        {
            this.authorName = authorName;
            string searchUrl = string.Format(WIKI_SEARCH_URL, ConstructWikiAuthorRequest(authorName));
            authorPageHtmlDocument = ProcessSearchPage(GetPageHtmlDocument(searchUrl));
        }

        private HtmlDocument ProcessSearchPage(HtmlDocument htmlDocument)
        {
            TypeOfWikiPage typeOfPage = GetTypeOfWikiPage(htmlDocument);

            var dataBlock = htmlDocument.DocumentNode.SelectSingleNode("//div[@class=\"searchresults\"]");

            switch (typeOfPage)
            {
                case TypeOfWikiPage.HitList:
                    {
                        authorPageUrl = dataBlock.SelectSingleNode("//p[@class=\"mw-search-createlink\"]").SelectSingleNode(".//a").GetAttributeValue("href", "");
                        authorPageUrl = WIKI_BASE_PAGE + authorPageUrl;
                        return GetPageHtmlDocument(authorPageUrl);
                    }
                case TypeOfWikiPage.AlternativesList:
                    {
                        //Пробуем первую ссылку
                        foreach (var searchResult in dataBlock.SelectNodes(".//div[@class=\"mw-search-result-heading\"]"))
                        {
                            if (searchResult != null && !searchResult.InnerText.ToLower().Contains("(значения)") && LinkIsContainAuthor(authorName, searchResult.InnerText))
                            {
                                foreach (HtmlNode node in searchResult.ChildNodes)
                                {
                                    if (node.Name.Equals("a"))
                                    {
                                        authorPageUrl = node.Attributes["href"].Value;
                                        authorPageUrl = WIKI_BASE_PAGE + authorPageUrl;
                                        htmlDocument = GetPageHtmlDocument(authorPageUrl);
                                        return ProcessSearchPage(htmlDocument);
                                    }
                                }
                            }
                        }
                        return null;
                    }
                case TypeOfWikiPage.ListOfNamesakes:
                case TypeOfWikiPage.Undefined:
                case TypeOfWikiPage.NothingFound:
                    {
                        return null;
                    }
            }
            return null;
        }

        private TypeOfWikiPage GetTypeOfWikiPage(HtmlDocument htmlDocument)
        {
            var dataBlock = htmlDocument.DocumentNode.SelectSingleNode("//div[@class=\"searchresults\"]");
            if (htmlDocument.DocumentNode.SelectSingleNode("//div[@class=\"dablink noprint\"]") != null &&
                htmlDocument.DocumentNode.SelectSingleNode("//div[@class=\"dablink noprint\"]").InnerText.Contains("В Википедии есть статьи о других людях с такой фамилией") &&
                 htmlDocument.DocumentNode.SelectSingleNode("//table[@class=\"notice metadata plainlinks\"]") != null)
            {
                return TypeOfWikiPage.ListOfNamesakes;
            }
            else if (dataBlock != null &&
                dataBlock.SelectSingleNode("//p[@class=\"mw-search-createlink\"]").InnerText.Contains(CREATE_PAGE_STR) &&
                dataBlock.SelectSingleNode("//div[@class=\"searchdidyoumean\"]") == null)
            {
                return TypeOfWikiPage.AlternativesList;
            }
            else if (dataBlock != null && !dataBlock.SelectSingleNode("//p[@class=\"mw-search-createlink\"]").InnerText.Contains(CREATE_PAGE_STR) &&
               dataBlock.SelectSingleNode("//p[@class=\"mw-search-createlink\"]").InnerText.Contains(PAGE_IS_EXIST))
            {
                return TypeOfWikiPage.HitList;
            }
            else if (dataBlock != null && dataBlock.SelectSingleNode("//p[@class=\"mw-search-nonefound\"]").InnerText.Contains("Соответствий запросу не найдено."))
            {
                return TypeOfWikiPage.NothingFound;
            }
            return TypeOfWikiPage.Undefined;
        }

        private bool LinkIsContainAuthor(string authorName, string authorLink)
        {
            bool result = true;
            string[] authorParts = RemoveSpareChars(authorName.Split(' '));
            string[] linkParts = RemoveSpareChars(authorLink.Split(','));
            bool[] used = new bool[linkParts.Length];
            for (int i = 0; i < used.Length; i++)
            {
                used[i] = false;
            }
            var sortedAuthorParts = from s in authorParts
                                    orderby s.Length descending
                                    select s;
            foreach (string authorPart in sortedAuthorParts)
            {
                bool itemExist = false;
                for (int i = 0; i < linkParts.Length; i++)
                {
                    if (authorPart.Length > 1)
                    {
                        if (linkParts[i].Equals(authorPart) && !used[i])
                        {
                            used[i] = true;
                            itemExist = true;
                            break;
                        }
                    }
                    else
                    {
                        if (linkParts[i].StartsWith(authorPart) && !used[i])
                        {
                            used[i] = true;
                            itemExist = true;
                            break;
                        }
                    }
                }

                if (!itemExist)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private string[] RemoveSpareChars(string[] parts)
        {
            List<string> result = new List<string>();
            foreach (string part in parts)
            {
                string tmpStr = part.Replace(".", "").Replace("(", "").Replace(")", "").Replace("ё", "е").Replace("Ё", "Е");
                if (tmpStr.Trim().Split(' ').Length > 1)
                {
                    foreach (string tmpPart in tmpStr.Trim().Split(' '))
                    {
                        result.Add(tmpPart.Trim());
                    }
                }
                else
                {
                    result.Add(part.Trim());
                }
            }
            return result.ToArray();
        }

        private HtmlDocument GetPageHtmlDocument(string pageUrl)
        {
            using (KnigoskopWebClient client = new KnigoskopWebClient())
            {
                string pageSources = client.DownloadData(pageUrl);
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(pageSources);
                return htmlDocument;
            }
        }

        private string ConstructWikiAuthorRequest(string AuthorName)
        {
            if (AuthorName.Trim().Contains(' '))
            {
                string tmpAuthorName = string.Empty;
                foreach (string part in AuthorName.Split(' '))
                {
                    tmpAuthorName += part.Trim() + " ";
                }
                return tmpAuthorName.Trim().Replace(' ', '+');
            }
            else
            {
                return AuthorName;
            }
        }
    }
}
