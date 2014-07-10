using System;
using System.Collections.Generic;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class AuthorModel : BaseItemModel
    {
        public IEnumerable<BaseItemModel> Series { get; set; }
        public string SourceUrl { get; set; }
        public DateTime? BornDate { get; set; }
        public DateTime? DeathDate { get; set; }
        public IEnumerable<BookItemModel> Books { get; set; }
        public string OfficialUrl { get; set; }
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Author; }
        }
    }
}