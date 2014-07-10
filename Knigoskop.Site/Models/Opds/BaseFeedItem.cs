using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Knigoskop.Site.Models.Opds
{
    public abstract class BaseFeedItem : BaseItem
    {
        public abstract DocumentTypeEnum Type { get; }
        public virtual Guid Id { get; set; }
        public virtual DateTime Updated { get; set; }
        public virtual string Title { get; set; }
        public virtual IEnumerable<BaseLink> Links { get; set; }
        public virtual IEnumerable<Author> Authors { get; set; }
        public virtual string Content { get; set; }

        public override XElement Render()
        {
            throw new NotImplementedException();
        }
    }
}