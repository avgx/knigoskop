using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace Knigoskop.Services.FB2Engine
{
    public partial class FB2Parser
    {
        private void ParseFB2DocumentInfo()
        {
            if (fb2XDocument.Root.Element(FB2_DESCRIPTION) != null && fb2XDocument.Root.Element(FB2_DESCRIPTION).Element(FB2_DOCUMENT_INFO) != null)
            {
                XElement documentInfo = fb2XDocument.Root.Element(FB2_DESCRIPTION).Element(FB2_DOCUMENT_INFO);

                if (documentInfo != null && documentInfo.Element("id") != null)
                {
                    fb2BookId = documentInfo.Element("id").Value;
                }
            }
        }

        private void ParseFB2PublishInfo()
        {
            if (fb2XDocument.Root.Element(FB2_DESCRIPTION) != null && fb2XDocument.Root.Element(FB2_DESCRIPTION).Element(FB2_PUBLISH_INFO) != null)
            {
                XElement publishInfoElement = fb2XDocument.Root.Element(FB2_DESCRIPTION).Element(FB2_PUBLISH_INFO);
                publishInfo.BookName = (publishInfoElement.Element("book-name") != null) ? publishInfoElement.Element("book-name").Value : "";
                publishInfo.Publisher = (publishInfoElement.Element("publisher") != null) ? publishInfoElement.Element("publisher").Value : "";
                publishInfo.City = (publishInfoElement.Element("city") != null) ? publishInfoElement.Element("city").Value : "";
                string yearStr = (publishInfoElement.Element("year") != null) ? publishInfoElement.Element("year").Value : "";
                int.TryParse(yearStr, out publishInfo.Year);
                if (publishInfoElement.Element("isbn") != null)
                {
                    publishInfo.ISBNs = GetISBNs(publishInfoElement.Element("isbn"));
                }
            }
        }

        private void ParseFB2TitleInfo()
        {
            if (fb2XDocument.Root.Element(FB2_DESCRIPTION) != null && fb2XDocument.Root.Element(FB2_DESCRIPTION).Element(FB2_TITLE_INFO) != null)
            {
                XElement title = fb2XDocument.Root.Element(FB2_DESCRIPTION).Element(FB2_TITLE_INFO);

                titleInfo.Genres = GetFB2Genres(title.Elements("genre"));
                titleInfo.Authors = GetFB2Authors(title.Elements("author"));
                titleInfo.BookTitle = (title.Element("book-title") != null) ? title.Element("book-title").Value : "";
                titleInfo.Annotation = string.Empty;
                if (title.Element("annotation") != null)
                {
                    ParseFB2Node((XNode)title.Element("annotation"), ref titleInfo.Annotation);
                }
                titleInfo.Date = (title.Element("date") != null) ? title.Element("date").Value : "";
                titleInfo.Translator = (title.Element("translator") != null) ? ParsePerson(title.Element("translator")) : new PersonName();
                ParseFB2Binaries();
                if (title.Element("coverpage") != null && title.Element("coverpage").Element("image") != null &&
                    title.Element("coverpage").Element("image").FirstAttribute != null)
                {
                    titleInfo.CoverPage = GetPictureFromFB2(title.Element("coverpage").Element("image").FirstAttribute.Value.Replace("#", ""));
                    coverBinaryName = title.Element("coverpage").Element("image").FirstAttribute.Value.Replace("#", "");
                }
                if (title.Element("sequence") != null)
                {
                    titleInfo.Sequence = GetSequence(title.Element("sequence"));
                }
            }
        }

        private void ParseFB2Binaries()
        {
            if (fb2XDocument.Root.Elements("binary") != null)
            {
                binaries = new List<FB2Binary>();
                foreach (XElement binary in fb2XDocument.Root.Elements("binary"))
                {
                    string binaryId = binary.Attribute("id").Value;
                    string contentType = string.Empty;
                    if (binary.Attribute("content-type") != null)
                    {
                        contentType = binary.Attribute("content-type").Value;
                    }
                    try
                    {
                        byte[] sources = Convert.FromBase64String(binary.Value);
                        binaries.Add(new FB2Binary(binaryId, sources, contentType));
                    }
                    catch
                    { }
                }
            }
        }

        private byte[] GetPictureFromFB2(string imageName)
        {
            foreach (FB2Binary fb2Binary in binaries)
            {
                if (fb2Binary.Id.ToLower().Equals(imageName.ToLower()))
                {
                    return fb2Binary.Sources;
                }
            }
            return null;
        }

        private PersonName[] GetFB2Authors(IEnumerable<XElement> authorsElements)
        {
            List<PersonName> authors = new List<PersonName>();
            foreach (XElement authorElement in authorsElements)
            {
                authors.Add(ParsePerson(authorElement));
            }
            //(title.Element("author") != null) ? ParsePerson(title.Element("author")) : new PersonName();
            return authors.ToArray();
        }

        private Sequence GetSequence(XElement sequenceElement)
        {
            Sequence result = new Sequence();
            if (sequenceElement.Attribute("name") != null)
            {
                result.Name = sequenceElement.Attribute("name").Value;
            }
            if (sequenceElement.Attribute("number") != null)
            {
                int.TryParse(sequenceElement.Attribute("number").Value, out result.Number);
            }
            return result;
        }

        private string[] GetFB2Genres(IEnumerable<XElement> genres)
        {
            List<string> genreResult = new List<string>();
            foreach (XElement genre in genres)
            {
                genreResult.Add(genre.Value);
            }
            return genreResult.ToArray();
        }

        private PersonName ParsePerson(XElement element)
        {
            string firstName = (element.Element("first-name") != null) ? element.Element("first-name").Value : "";
            string middleName = (element.Element("middle-name") != null) ? element.Element("middle-name").Value : "";
            string lastName = (element.Element("last-name") != null) ? element.Element("last-name").Value : "";
            return new PersonName(firstName, middleName, lastName);
        }

        private void ParseFB2Body()
        {
            XElement body = fb2XDocument.Root.Element(FB2_BODY);
            if (body != null)
            {
                ParseFB2Node((XNode)body, ref bookText, fb2TextSections);
            }
            else
            {
                throw new ArgumentException("FB2 file has incorrect format.");
            }
        }

        private void ParseFB2Node(XNode node, ref string targetStr)
        {
            ParseFB2Node(node, ref targetStr, null);
        }

        private void ParseFB2Node(XNode node, ref string targetStr, string[] allowedFB2Elements)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                if (((XElement)node).Name.LocalName.Equals("p") && !((XElement)node).HasElements)
                {
                    targetStr += ((XElement)node).Value;
                    targetStr += "\r\n";
                }
                if (((XElement)node).HasElements && (allowedFB2Elements == null || allowedFB2Elements.Contains(((XElement)node).Name.LocalName.ToLower())))
                {
                    foreach (XNode intNode in ((XElement)node).Nodes())
                    {
                        ParseFB2Node(intNode, ref targetStr, allowedFB2Elements);
                    }
                }
            }
        }
    }
}
