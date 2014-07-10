using System;
using ProtoBuf;

namespace Knigoskop.Site.Models.Shared
{
    [ProtoContract]
    public enum ItemTypeEnum
    {
        [ProtoEnum]
        Book = 0,
        [ProtoEnum]
        Author = 1,
        [ProtoEnum]
        Serie = 2,
        [ProtoEnum]
        Review = 3,
        [ProtoEnum]
        User = 4
    }

    [ProtoContract]
    public enum GenreLevelEnum
    {
        [ProtoEnum]
        Root,
        [ProtoEnum]
        Children
    }

    public enum ComplainType
    {
        Review,
        Comment
    }

    public enum CatalogueViewTypeEnum
    {
        Books,
        Authors
    }

    public enum ResultTypeEnum
    {
        All,
        Search,
        Genre
    }

    public enum ReviewsViewTypeEnum
    {
        List,
        Short
    }


    public enum IncomeStateEnum
    {
        New,
        Update
    }

    public enum BookFormatEnum
    {
        FB2,
        EPUB,        
        MOBI,
        AZW3,
        RTF,
        TXT
    }

    public enum DownloadTypeEnum
    {
        File,
        Email
    }


}