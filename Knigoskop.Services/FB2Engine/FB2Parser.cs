using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using FB2Fix;
using Knigoskop.Services.Logger;

namespace Knigoskop.Services.FB2Engine
{
    public partial class FB2Parser
    {
        public const string FB2_BAD_FOLDER = "Bad";
        public const string FB2_BODY = "body";
        public const string FB2_TITLE = "title";
        public const string FB2_SECTION = "section";
        public const string FB2_SUBTITLE = "subtitle";
        public const string FB2_TITLE_INFO = "title-info";
        public const string FB2_DOCUMENT_INFO = "document-info";
        public const string FB2_PUBLISH_INFO = "publish-info";
        public const string FB2_DESCRIPTION = "description";
        public const string FB2_FIRSTNAME = "first-name";
        public const string FB2_MIDDLENAME = "middle-name";
        public const string FB2_LASTNAME = "first-name";

        private XDocument fb2XDocument;
        private string[] fb2TextSections = new string[] { FB2_BODY, FB2_TITLE, FB2_SECTION, FB2_SUBTITLE };
        private string bookText;
        private Encoding fb2Encoding;
        private TitleInfo titleInfo = new TitleInfo();
        private PublishInfo publishInfo = new PublishInfo();
        private bool shouldParseFB2Body = true;
        private string fb2BookId = string.Empty;
        private List<FB2Binary> binaries;
        private string coverBinaryName = string.Empty;
        private byte[] fb2Sources;

        public byte[] Fb2Sources
        {
            get { return fb2Sources; }
        }

        public Guid Fb2BookId
        {
            get
            {
                try
                {
                    return new Guid(fb2BookId);
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        public PublishInfo PublishInfo
        {
            get { return publishInfo; }
        }

        public TitleInfo TitleInfo
        {
            get { return titleInfo; }
        }

        public Encoding Fb2Encoding
        {
            get { return fb2Encoding; }
        }

        public string[] Fb2TextSections
        {
            get { return fb2TextSections; }
            set { fb2TextSections = value; }
        }

        public string BookText
        {
            get { return bookText; }
        }

        public FB2Binary[] BookBinaries
        {
            get
            {
                if (binaries != null)
                {
                    List<FB2Binary> result = new List<FB2Binary>();
                    foreach (FB2Binary fb2Binary in binaries)
                    {
                        if (!fb2Binary.Id.ToLower().Equals(coverBinaryName.ToLower()))
                        {
                            result.Add(fb2Binary);
                        }
                    }
                    return result.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        public byte[] GetBookSources(Encoding encoding)
        {
            if (!string.IsNullOrEmpty(bookText))
            {
                bool isUnicode = false;
                if (encoding == Encoding.Unicode)
                {
                    isUnicode = true;
                }
                byte[] fb2Sources = encoding.GetBytes(bookText);
                byte[] resultSources;
                if (!isUnicode)
                {
                    resultSources = fb2Sources;
                }
                else
                {
                    resultSources = new byte[fb2Sources.Length + 2];
                    byte[] unicodeHeader = new byte[2] { 255, 254 };
                    unicodeHeader.CopyTo(resultSources, 0);
                    fb2Sources.CopyTo(resultSources, 2);
                }
                //SaveTextFile(Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + ".txt", resultSources);
                return resultSources;
            }
            else
            {
                return null;
            }
        }

        public FB2Parser(string fb2FileName)
        {
            InitialInstance(fb2FileName, true);
        }

        public FB2Parser(string fb2FileName, bool shouldParseFB2Body)
        {
            InitialInstance(fb2FileName, shouldParseFB2Body);
        }

        private void InitialInstance(string fb2FileName, bool shouldParseFB2Body)
        {
            ApplicationLogger.WriteStringToLog(string.Format("Parsing fb2 file: {0}", Path.GetFileName(fb2FileName)));
            Encoding fb2Encoding = GetFB2Encoding(fb2FileName);
            this.shouldParseFB2Body = shouldParseFB2Body;
            LoadFB2ToXDocument(fb2FileName, 0);
            if (fb2XDocument == null)
            {
                throw new ArgumentException(string.Format("Can't load fb2 file: {0}", fb2FileName));
            }
            ParseFB2Fields();
        }

        private void ParseFB2Fields()
        {
            ParseFB2TitleInfo();
            ParseFB2DocumentInfo();
            ParseFB2PublishInfo();
            if (shouldParseFB2Body)
            {
                ParseFB2Body();
            }
        }

        private string[] GetISBNs(XElement isbnElement)
        {
            return isbnElement.Value.Split(',');
        }

        /*private void StoreImageToFile(byte[] p)
 {
     using (BinaryWriter bw = new BinaryWriter(new FileStream("test.jpeg", FileMode.Create, FileAccess.Write)))
     {
         bw.Write(p);
     }
 }*/

        private void LoadFB2ToXDocument(string fb2FileName, int itteration)
        {
            using (FB2StreamReader sr = new FB2StreamReader(fb2FileName, fb2Encoding))
            {
                try
                {
                    fb2XDocument = XDocument.Load(sr);
                }
                catch
                {
                    sr.Close();
                    if (itteration == 0)
                    {
                        TryEncodeFixAndLoad(fb2FileName, itteration);
                    }
                    else
                    {
                        throw new Exception(string.Format("Can't load fb2 file: {0}", fb2FileName));
                    }
                }
            }
            fb2Sources = File.ReadAllBytes(fb2FileName);
        }

        private void TryEncodeFixAndLoad(string fb2FileName, int itteration)
        {
            string fb2EncodedFile = fb2FileName;
            if (fb2Encoding != Encoding.UTF8)
            {
                fb2EncodedFile = EncodeFB2File(fb2FileName);
                fb2Encoding = Encoding.UTF8;
            }
            try
            {
                BatchFilesProcessor fb2Fix = new BatchFilesProcessor();
                fb2Fix.Process(fb2EncodedFile);
                itteration++;
                LoadFB2ToXDocument("Temp\\" + Path.GetFileName(fb2EncodedFile), itteration);
            }
            catch
            {
                CopyFB2ToBadFolder("Temp\\" + Path.GetFileName(fb2EncodedFile));
            }
            finally
            {
                //File.Delete(fb2EncodedFile);
                File.Delete("Temp\\" + Path.GetFileName(fb2EncodedFile));
            }
        }

        private void CopyFB2ToBadFolder(string fb2FileName)
        {
            if (!Directory.Exists(FB2_BAD_FOLDER))
            {
                Directory.CreateDirectory(FB2_BAD_FOLDER);
            }
            if (File.Exists(fb2FileName))
            {
                ApplicationLogger.WriteStringToLog(string.Format("Move file \"{0}\" to Bad folder.", Path.GetFileName(fb2FileName)));
                File.Copy(fb2FileName, FB2_BAD_FOLDER + "\\" + Path.GetFileName(fb2FileName));
            }
        }

        private string EncodeFB2File(string fb2FileName)
        {
            string fb2EncodedFileName = Path.GetFileNameWithoutExtension(fb2FileName) + ".fb2";
            using (FB2StreamReader sr = new FB2StreamReader(fb2FileName, fb2Encoding))
            {
                string line;
                using (StreamWriter sw = new StreamWriter(fb2EncodedFileName))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("<?xml version"))
                        {
                            line = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                        }
                        sw.WriteLine(line);
                    }
                }
            }
            return fb2EncodedFileName;
        }

        private Encoding GetFB2Encoding(string fileName)
        {
            const string ENCODING_STR = "encoding=";
            fb2Encoding = Encoding.Default;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line = sr.ReadLine();
                if (line.StartsWith("<?") && line.Contains(ENCODING_STR) && line.Contains("?>"))
                {
                    string encodingStr = line.Substring(0, line.IndexOf("?>") - 1).Substring(2);
                    encodingStr = encodingStr.Substring(encodingStr.IndexOf(ENCODING_STR) + ENCODING_STR.Length).Replace("\"", "");
                    if (encodingStr.Contains(" "))
                    {
                        encodingStr = encodingStr.Substring(0, encodingStr.IndexOf(" "));
                    }
                    fb2Encoding = Encoding.GetEncoding(encodingStr);
                }
            }
            return fb2Encoding;
        }
    }
}
