using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.Site.Common.Mvc;

namespace Knigoskop.Site.Models
{
    public class CitationModel : UniqueIdentifierModel
    {
        public string Text { get; set; }
        public Guid BookId { get; set; }
        public Guid UserId { get; set; }
        public DateTime Created { get; set; }
    }
}