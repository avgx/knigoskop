using HtmlAgilityPack;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace Knigoskop.Services
{
    public class AuthorData
    {
        private byte[] authorImage;
        private string biography;
        private DateTime? bornDate;
        private DateTime? deathDate;
        private string originalUrl;
        private string nameFromWiki;

        public string NameFromWiki
        {
            get { return nameFromWiki; }
        }

        public string OriginalUrl
        {
            get { return originalUrl; }
        }

        public DateTime? DeathDate
        {
            get { return deathDate; }
        }

        public DateTime? BornDate
        {
            get { return bornDate; }
        }

        public string Biography
        {
            get { return biography; }
        }

        public byte[] AuthorImage
        {
            get { return authorImage; }
        }

        public AuthorData(string originalUrl, HtmlDocument htmlDocument, HtmlNode vCard)
        {
            this.originalUrl = originalUrl;
            ProcessVCard(htmlDocument, vCard);
        }

        private void ProcessVCard(HtmlDocument htmlDocument, HtmlNode vCard)
        {
            authorImage = ProcessImageSource(vCard, htmlDocument);
            nameFromWiki = ProcessAuthorName(htmlDocument);
            biography = ProcessDescription(htmlDocument);
            biography += ProcessBiograhy(htmlDocument);
            if (vCard != null)
            {
                bornDate = ProcessDates("Дата рождения:", vCard);
                deathDate = ProcessDates("Дата смерти:", vCard);
            }
        }

        private string ProcessAuthorName(HtmlDocument htmlDocument)
        {
            string result = string.Empty;
            if (htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class=\"firstHeading\"]") != null)
            {
                result = htmlDocument.DocumentNode.SelectSingleNode(".//h1[@class=\"firstHeading\"]").InnerText;
            }
            result = result.Replace(",", "");

            return result;
        }

        private string ProcessDescription(HtmlDocument htmlDocument)
        {
            string tmpBiography = string.Empty;
            bool processFlag = false;

            foreach (HtmlNode node in htmlDocument.DocumentNode.SelectSingleNode("//div[@id=\"mw-content-text\"]").ChildNodes)
            {
                if (node != null && !(node.Name.ToLower().Equals("p") || node.Name.ToLower().Equals("#text")) && processFlag)
                {
                    processFlag = false;
                    break;
                }
                if (processFlag && node != null && node.Name.ToLower().Equals("ul"))
                {
                    tmpBiography += node.InnerText + "\r\n";
                }
                if (!processFlag && node != null && node.Name.ToLower().Equals("p"))
                {
                    tmpBiography += node.InnerText + "\r\n";
                    processFlag = true;
                }

            }
            tmpBiography = ClearTextFromTrash(tmpBiography);
            return tmpBiography;
        }

        private DateTime? ProcessDates(string rowLabel, HtmlNode vCard)
        {
            foreach (HtmlNode node in vCard.ChildNodes)
            {
                if (node.InnerText.ToLower().Contains(rowLabel.ToLower()))
                {
                    string dateStr = node.InnerText.Replace(rowLabel, "").Trim();
                    dateStr = RemoveBrackets(dateStr);
                    DateTime resultDate;
                    IFormatProvider culture = new CultureInfo("ru-Ru");
                    if (DateTime.TryParse(dateStr, culture, DateTimeStyles.AssumeLocal, out resultDate))
                    {
                        return resultDate;
                    }
                }
            }
            return null;
        }

        private string RemoveBrackets(string dateStr)
        {
            if (dateStr.Contains("(") && dateStr.Contains(")"))
            {
                dateStr = dateStr.Substring(0, dateStr.IndexOf('(')) + dateStr.Substring(dateStr.IndexOf(')') + 1);
                return RemoveBrackets(dateStr);
            }
            else
            {
                return dateStr;
            }
        }

        private string ProcessBiograhy(HtmlDocument htmlDocument)
        {
            string tmpBiography = string.Empty;
            bool processFlag = false;

            foreach (HtmlNode node in htmlDocument.DocumentNode.SelectSingleNode("//div[@id=\"mw-content-text\"]").ChildNodes)
            {
                if (node != null && node.Name.ToLower().Equals("h2") && processFlag)
                {
                    processFlag = false;
                    if (!string.IsNullOrEmpty(tmpBiography))
                    {
                        tmpBiography = tmpBiography.Substring(0, tmpBiography.Length - 2);
                    }
                    break;
                }
                if (node != null && node.Name.ToLower().Equals("h2") && node.InnerText.Contains("Биография"))
                {
                    tmpBiography += "\r\nБиография\r\n";
                    processFlag = true;
                }
                if (processFlag && node != null && node.Name.ToLower().Equals("p"))
                {
                    tmpBiography += node.InnerText + "\r\n";
                }
                if (processFlag && node != null && node.Name.ToLower().Equals("ul"))
                {
                    tmpBiography += node.InnerText + "\r\n";
                }
                if (processFlag && node != null && node.Name.ToLower().Equals("h3"))
                {
                    tmpBiography += "\r\n" + node.InnerText + "\r\n";
                }
            }
            tmpBiography = ClearTextFromTrash(tmpBiography);
            return tmpBiography;
        }

        private string ClearTextFromTrash(string text)
        {
            text = text.Replace("&#160;", " ");
            text = text.Replace("[править]", "");

            return text;
        }

        private byte[] ProcessImageSource(HtmlNode vCard, HtmlDocument htmlDocument)
        {
            string imageUrl = string.Empty;
            if (vCard != null)
            {
                imageUrl = vCard.SelectSingleNode(".//a[@class=\"image\"]/img").Attributes["src"].Value;
            }
            else
            {
                //try to get image from page

                if (htmlDocument.DocumentNode.SelectSingleNode(".//a[@class=\"image\"]") != null &&
                    htmlDocument.DocumentNode.SelectSingleNode(".//a[@class=\"image\"]/img[@class=\"thumbimage\"]") != null)
                {
                    imageUrl = htmlDocument.DocumentNode.SelectSingleNode(".//a[@class=\"image\"]/img[@class=\"thumbimage\"]").Attributes["src"].Value;
                }
            }
            if (!string.IsNullOrEmpty(imageUrl))
            {
                if (!imageUrl.ToLower().StartsWith("http://"))
                {
                    imageUrl = "http:" + imageUrl;
                }
                KnigoskopWebClient webClient = new KnigoskopWebClient();
                byte[] imageSource = webClient.DownloadImage(imageUrl);
                if (imageSource != null)
                {
                    MemoryStream ms = new MemoryStream(imageSource);
                    Image image = Image.FromStream(ms);
                    ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Jpeg);
                    imageSource = ms.ToArray();

                    /*using (FileStream fs = new FileStream("TestImage.jpeg", FileMode.Create, FileAccess.Write))
                    {
                        BinaryWriter bw = new BinaryWriter(fs);
                        bw.Write(imageSource);
                        bw.Flush();
                    }*/
                }

                return imageSource;
            }
            return null;
        }
    }
}
