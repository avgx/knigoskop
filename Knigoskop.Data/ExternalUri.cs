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
    
    public partial class ExternalUri
    {
        public System.Guid BookId { get; set; }
        public System.Guid ExternalResourceId { get; set; }
        public string Uri { get; set; }
    
        public virtual Book Book { get; set; }
        public virtual ExternalResource ExternalResource { get; set; }
    }
}