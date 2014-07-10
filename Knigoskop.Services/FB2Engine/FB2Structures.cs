using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knigoskop.Services.FB2Engine
{
    public struct FB2Binary
    {
        public string Id;
        public byte[] Sources;
        public string ContentType;

        public FB2Binary(string id, byte[] sources, string contentType)
        {
            Id = id;
            Sources = sources;
            ContentType = contentType;
        }
    }

    public struct PublishInfo
    {
        public string BookName;
        public string Publisher;
        public string City;
        public int Year;
        public string[] ISBNs;
    }

    public struct TitleInfo
    {
        public string[] Genres;
        public PersonName[] Authors;
        public string BookTitle;
        public string Annotation;
        public string Date;
        public PersonName Translator;
        public byte[] CoverPage;
        public Sequence Sequence;
    }

    public struct Sequence
    {
        public string Name;
        public int Number;

        public Sequence(string name, int number)
        {
            Name = name;
            Number = number;
        }
    }

    public struct PersonName
    {
        public string FirstName;
        public string MiddleName;
        public string LastName;

        public string FullName
        {
            get
            {
                string tmpStr = string.Empty;
                if (!string.IsNullOrEmpty(LastName))
                {
                    tmpStr += LastName.Trim() + " ";
                }
                if (!string.IsNullOrEmpty(FirstName))
                {
                    tmpStr += FirstName.Trim() + " ";
                }
                if (!string.IsNullOrEmpty(MiddleName))
                {
                    tmpStr += MiddleName.Trim() + " ";
                }
                tmpStr = tmpStr.Trim();
                return tmpStr;
            }
        }

        public PersonName(string firstName, string middleName, string lastName)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
        }
    }
}
