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
    
    public partial class OpdsLink
    {
        public System.Guid OpdsLinkId { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string SearchUri { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
    
        public virtual User User { get; set; }
    }
}
