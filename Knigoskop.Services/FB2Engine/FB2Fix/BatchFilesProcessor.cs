// Copyright (c) 2007-2012 Andrej Repin aka Gremlin2
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

using FB2Fix.ObjectModel;
using FB2Fix.Sgml;
using Knigoskop.Services.Logger;


namespace FB2Fix
{
    internal class BatchFilesProcessor
    {
        //private Dictionary<string, bool> excludeList;

        //private readonly string outputDirectoryRoot;
        private readonly string outputDirectoryGood;
        private readonly string outputDirectoryBad;
        //private readonly string outputDirectoryNonValid;


        private static readonly Regex bullets;
        private static readonly Regex invalidChars;
        private SgmlDtd fb2Dtd;
        private int? preferedCodepage;

        //private XmlSchemaSet xsdSchema;

        static BatchFilesProcessor()
        {
            bullets = new Regex("[\u0001-\u0008]");
            invalidChars = new Regex("[\u000b\u000c\u000e-\u001f]");
        }

        private void ProcessElement(FictionBook fictionBook, XmlNode node, List<XmlElement> invalidNodes)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                ProcessElement(fictionBook, childNode, invalidNodes);
            }

            switch (node.NodeType)
            {
                case XmlNodeType.Text:
                    string text = node.InnerText;

                    text = bullets.Replace(text, new MatchEvaluator(delegate(Match match)
                    {
                        fictionBook.ModificationType = ModificationType.Text;
                        return "-";
                    }));

                    text = invalidChars.Replace(text, new MatchEvaluator(delegate(Match match)
                    {
                        fictionBook.ModificationType = ModificationType.Text;
                        return " ";
                    }));

                    node.InnerText = text;

                    //node.InnerText = invalidChars.Replace(bullets.Replace(node.InnerText, "-"), " ");
                    break;

                case XmlNodeType.Element:
                    ElementDecl elementDecl = this.fb2Dtd.FindElement(node.LocalName);
                    if (elementDecl == null)
                    {
                        invalidNodes.Add(node as XmlElement);
                    }
                    break;
            }
        }

        private void PostProcessDocument(FictionBook fictionBook)
        {
            List<XmlElement> invalidNodes;

            if (fictionBook == null)
            {
                throw new ArgumentNullException("fictionBook");
            }

            XmlDocument document = fictionBook.Document;

            invalidNodes = new List<XmlElement>(64);
            ProcessElement(fictionBook, document.DocumentElement, invalidNodes);

            //foreach (XmlElement node in invalidNodes)
            //{
            //    if(node == null)
            //    {
            //        continue;
            //    }

            //    XmlElement parent = node.ParentNode as XmlElement;
            //    if (parent != null && parent.NodeType == XmlNodeType.Element)
            //    {
            //        XmlComment comment = parent.OwnerDocument.CreateComment(node.OuterXml);
            //        parent.ReplaceChild(comment, node);
            //    }
            //}

            XmlNodeList nodes = document.SelectNodes("//FictionBook/descendant::p");
            List<XmlElement> paragraphNodes = new List<XmlElement>(nodes.Count);

            foreach (XmlNode node in nodes)
            {
                XmlElement paragraph = node as XmlElement;
                if (paragraph != null)
                {
                    paragraphNodes.Add(paragraph);
                }
            }

            for (int index = paragraphNodes.Count - 1; index >= 0; index--)
            {
                XmlElement paragraphElement = paragraphNodes[index];
                XmlElement parentElement = paragraphElement.ParentNode as XmlElement;
                if (parentElement != null && String.Compare(parentElement.LocalName, "p") == 0)
                {
                    XmlElement precedingElement = parentElement.ParentNode as XmlElement;
                    if (precedingElement != null)
                    {
                        parentElement.RemoveChild(paragraphElement);
                        precedingElement.InsertAfter(paragraphElement, parentElement);
                        fictionBook.ModificationType = ModificationType.Body;
                    }
                }
            }
        }

        private FictionBook ReadFictionBook(TextReader stream)
        {
            SgmlReader reader = new SgmlReader();
            reader.InputStream = stream;

            if (this.fb2Dtd == null)
            {
                reader.SystemLiteral = "fb2.dtd";
                this.fb2Dtd = reader.Dtd;
            }
            else
            {
                reader.Dtd = this.fb2Dtd;
            }

            FictionBook fictionBook = ReadFictionBook(reader);

            if (reader.MarkupErrorsCount > 0)
            {
                fictionBook.ModificationType = ModificationType.Body;
            }

            return fictionBook;
        }

        private FictionBook ReadFictionBook(XmlReader reader)
        {
            XmlDocument document = new XmlDocument();
            document.Load(reader);

            FictionBook fictionBook = new FictionBook(document);

            if (fictionBook.DocumentStatus == Fb2FixStatus.Passed)
            {
                return fictionBook;
            }

            fictionBook.CheckDocumentHeader();

            PostProcessDocument(fictionBook);

            fictionBook.DocumentStatus = Fb2FixStatus.Passed;

            return fictionBook;
        }

        private void ChangeDocumentVersion(FictionBook fictionBook)
        {
            if ((fictionBook.ModificationType & ModificationType.DocumentInfo) == ModificationType.DocumentInfo)
            {
                fictionBook.Version = 1.0f;
                return;
            }

        }

        private string GetOutputFileName(string path, string filename, string extension)
        {
            string fullFilename;

            fullFilename = Path.Combine(path, filename) + extension;

            int fileIndex = 0;
            while (File.Exists(fullFilename))
            {
                fileIndex++;
                string suffix = fileIndex.ToString(CultureInfo.InvariantCulture);

                fullFilename = Path.Combine(path, filename) + suffix + extension;
            }

            return fullFilename;
        }

        private string GetFilename(string directory, string filename, FictionBook fictionBook)
        {
            filename = filename.ToLowerInvariant();

            return filename;
        }

        private static void StreamCopy(Stream source, Stream destination)
        {
            if (source != null && destination != null)
            {
                byte[] buffer = new byte[4096];

                int readed;

                do
                {
                    readed = source.Read(buffer, 0, buffer.Length);

                    if (readed < 0)
                    {
                        throw new EndOfStreamException("Unexpected end of stream");
                    }

                    destination.Write(buffer, 0, readed);
                } while (readed > 0);
            }
        }

        private void SaveFictionBook(string directory, string filename, FictionBook fictionBook, Encoding encoding)
        {
            string outputFilename = String.Empty;
            XmlDocument document = fictionBook.Document;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                outputFilename = GetOutputFileName(directory, filename, ".fb2");

                using (Fb2TextWriter writer = new Fb2TextWriter(outputFilename, encoding))
                {

                    writer.WriteStartDocument();

                    document.WriteTo(writer);
                    writer.Flush();
                }

                if (!String.IsNullOrEmpty(outputFilename))
                {
                    DateTime dt = fictionBook.ContainerDateTime;

                    if (!dt.IsDaylightSavingTime())
                    {
                        dt = dt.AddHours(-1);
                    }

                    File.SetCreationTime(outputFilename, dt);
                    File.SetLastAccessTime(outputFilename, dt);
                    File.SetLastWriteTime(outputFilename, dt);
                }
            }
            catch (Exception)
            {
                if (!String.IsNullOrEmpty(outputFilename))
                {
                    if (File.Exists(outputFilename))
                    {
                        try
                        {
                            File.Delete(outputFilename);
                        }
                        catch (Exception exp)
                        {
                            ApplicationLogger.WriteStringToLog(exp.Message);
                        }
                    }
                }
                throw;
            }
        }

        private void ProcessDocument(Stream stream, string filename, DateTime lastModifiedTime)
        {
            Encoding encoding = null;
            FictionBook document = null;

            ApplicationLogger.WriteStringToLog(string.Format("Processing fb2 document '{0}'.", filename));

            try
            {
                using (HtmlStream htmlStream = new HtmlStream(stream, Encoding.Default))
                {
                    encoding = htmlStream.Encoding;
                    document = ReadFictionBook(htmlStream);

                    ChangeDocumentVersion(document);

                    if (document.ModificationType == ModificationType.None)
                    {
                        document.ContainerDateTime = lastModifiedTime;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                throw new Exception("InvalidFictionBookFormatException(exp.Message, exp)");
            }
            catch (XmlException)
            {
                throw new Exception("InvalidFictionBookFormatException(exp.Message, exp)");
            }

            try
            {
                if (encoding == null)
                {
                    throw new Exception("Can't detect a character encoding.");
                }

                long threshold = (long)(document.Document.InnerText.Length * 0.25);

                if (this.preferedCodepage != null)
                {
                    encoding = Encoding.GetEncoding((int)this.preferedCodepage, new EncoderCharEntityFallback(threshold), new DecoderExceptionFallback());
                }
                else if (encoding.IsSingleByte)
                {
                    encoding = Encoding.GetEncoding(encoding.CodePage, new EncoderCharEntityFallback(threshold), new DecoderExceptionFallback());
                }

                bool done = false;
                int retryCount = 0;

                do
                {
                    try
                    {
                        if (++retryCount > 2)
                        {
                            break;
                        }

                        if (encoding != null && document != null)
                        {
                            string outputFullPath = GetFilename(this.outputDirectoryGood, filename, document);
                            string outputDirectory = "Temp";
                            string outputFilename = Path.GetFileName(outputFullPath).Trim();

                            SaveFictionBook(outputDirectory, outputFilename, document, encoding);
                        }

                        done = true;
                    }
                    catch (EncoderFallbackException)
                    {
                        if (encoding != null)
                        {
                            ApplicationLogger.WriteStringToError(string.Format("Invalid document encoding ({0}) detected, utf-8 is used instead.", encoding.WebName));
                        }

                        encoding = Encoding.UTF8;
                    }
                }
                while (!done);
            }
            catch (IOException exp)
            {
                ApplicationLogger.WriteStringToError(exp.Message);
                Environment.Exit(1);
            }
            catch (UnauthorizedAccessException exp)
            {
                ApplicationLogger.WriteStringToError(exp.Message);
            }
        }

        private void ProcessFile(string filename)
        {
            string extension = Path.GetExtension(filename);

            if (String.Compare(extension, ".fb2", true) == 0)
            {
                using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        TimeSpan localOffset = now - now.ToUniversalTime();

                        ProcessDocument(fileStream, Path.GetFileNameWithoutExtension(filename), File.GetLastWriteTimeUtc(filename) + localOffset);
                    }
                    catch (Exception exp)
                    {
                        ApplicationLogger.WriteStringToError(exp.Message);
                        try
                        {
                            string outputFilename = GetOutputFileName(this.outputDirectoryBad, Path.GetFileNameWithoutExtension(filename), ".fb2");
                            File.Copy(filename, outputFilename);
                        }
                        catch (Exception e)
                        {
                            ApplicationLogger.WriteStringToError(e.Message);
                        }
                    }
                }
            }
        }


        public void Process(string fb2FileName)
        {
            if (File.Exists(fb2FileName))
            {
                ProcessFile(fb2FileName);
            }
            else
            {
                ApplicationLogger.WriteStringToError(string.Format("{0} is not a valid file or directory.", fb2FileName));
            }
        }
    }
}
