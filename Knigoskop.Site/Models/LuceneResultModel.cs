using System;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Models
{
    public class LuceneResultModel
    {
        public Guid Id { get; set; }
        public ItemTypeEnum Type { get; set; }
        public float Score { get; set; }
    }
}