using System;
using System.Collections.Generic;
using Knigoskop.Site.Common.Mvc;

namespace Knigoskop.Site.Models
{
    public class GenreModel : UniqueIdentifierModel
    {
        public string Name { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsSelected { get; set; }
        public IEnumerable<GenreModel> Children { get; set; }
    }
}