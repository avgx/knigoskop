using System;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class CommentModel : UniqueIdentifierModel
    {
        public ItemTypeEnum Type { get; set; }
        public Guid? ParentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime UserImageUpdated { get; set; }
        public DateTime Created { get; set; }
        public string Text { get; set; }
        public bool IsPublished { get; set; }
        public bool HasChildren { get; set; }
    }
}