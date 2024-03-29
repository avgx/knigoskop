//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Knigoskop.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Income
    {
        public System.Guid IncomeId { get; set; }
        public string Body { get; set; }
        public System.DateTime Created { get; set; }
        public System.Guid UserId { get; set; }
        public Nullable<System.Guid> BookId { get; set; }
        public Nullable<System.Guid> SerieId { get; set; }
        public Nullable<System.Guid> AuthorId { get; set; }
        public bool IsPublished { get; set; }
        public int ItemType { get; set; }
        public string Name { get; set; }
        public Nullable<System.Guid> ProcessedByUserId { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual Book Book { get; set; }
        public virtual Serie Series { get; set; }
        public virtual User User { get; set; }
        public virtual User ProcessedBy { get; set; }
    }
}
