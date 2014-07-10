using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Knigoskop.Services.FB2Engine
{
    //http://stackoverflow.com/questions/2086465/reading-xml-with-xdocument-problem
    public class FB2StreamReader : StreamReader
    {
        public FB2StreamReader(string path)
            : base(path)
        {

        }

        public FB2StreamReader(string path, bool detectEncodingFromByteOrderMarks)
            : base(path, detectEncodingFromByteOrderMarks)
        {
        }

        public FB2StreamReader(string path, Encoding encoding)
            : base(path, encoding)
        {
        }


        public override int Read(char[] buffer, int index, int count)
        {
            int intResult;
            intResult = base.Read(buffer, index, count);
            string line = new string(buffer);
            if (line.Contains("xmlns=\"http://www.gribuser.ru/xml/fictionbook/2.0\""))
            {
                line = line.Replace("xmlns=\"http://www.gribuser.ru/xml/fictionbook/2.0\"", "xmlns:xhtml =\"http://gribuser/xml/fictionbook/2.0\"");
                line.ToCharArray().CopyTo(buffer, 0);
            }
            return intResult;
        }
    }
}
