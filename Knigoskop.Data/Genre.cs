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
    
    public partial class Genre
    {
        public Genre()
        {
            this.Child = new HashSet<Genre>();
            this.Books = new HashSet<Book>();
        }
    
        public System.Guid GenreId { get; set; }
        public Nullable<System.Guid> ParentId { get; set; }
        public string Code { get; set; }
        public string Classifier { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Genre> Child { get; set; }
        public virtual Genre Parent { get; set; }
        public virtual ICollection<Book> Books { get; set; }
    }
}
