using System;
using System.Collections.Generic;
using Knigoskop.Site.Common.Mvc;
using Knigoskop.Site.Models.Shared;
using OAuth2.Models;

namespace Knigoskop.Site.Models
{
    public abstract class BaseItemModel : UniqueIdentifierModel
    {
        public bool HasImage { get; set; }
        public virtual string Name { get; set; }
        public string Description { get; set; }
        public abstract ItemTypeEnum ItemType { get; }
        public IEnumerable<CommentModel> Comments { get; set; }
        public RatingModel Rating { get; set; }
        public ViewStatsModel ViewStats { get; set; }
        public bool IsUserSubscribed { get; set; }
        public DateTime Created { get; set; }
        public IdentityInfoModel CreatedBy { get; set; }
    }
}