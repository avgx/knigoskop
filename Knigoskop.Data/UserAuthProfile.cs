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
    
    public partial class UserAuthProfile
    {
        public System.Guid UserAuthProfileId { get; set; }
        public System.Guid UserId { get; set; }
        public string ProviderName { get; set; }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public System.DateTime IsAttached { get; set; }
    
        public virtual User User { get; set; }
    }
}
