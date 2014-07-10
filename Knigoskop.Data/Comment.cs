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
    
    public partial class Comment
    {
        public Comment()
        {
            this.Children = new HashSet<Comment>();
            this.Complains = new HashSet<Complain>();
        }
    
        public System.Guid CommentId { get; set; }
        public System.DateTime Created { get; set; }
        public System.Guid UserId { get; set; }
        public bool IsPublished { get; set; }
        public string Text { get; set; }
        public Nullable<System.Guid> ParentId { get; set; }
        public Nullable<System.Guid> BookId { get; set; }
        public Nullable<System.Guid> AuthorId { get; set; }
        public Nullable<System.Guid> SerieId { get; set; }
        public Nullable<System.Guid> ReviewId { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual Book Book { get; set; }
        public virtual Review Review { get; set; }
        public virtual Serie Series { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Comment> Children { get; set; }
        public virtual Comment Parent { get; set; }
        public virtual ICollection<Complain> Complains { get; set; }
    }
}
